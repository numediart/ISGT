using System;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Data_Classes;
using UnityEngine.SceneManagement;
using Utils;
using Random = System.Random;

public class DatabaseGenerator : MonoBehaviour
{
    #region Public Fields
    
    public DatabaseGenerationScriptableObject DatabaseGenerationData;
    
    #endregion

    #region Private Fields

    private float _timeBetweenScreenshots;
    private string _openingsDataFolderPath;
    private Random _random;
    private List<Bounds> _emptyQuadNodesCenters;
    private TimeTools _timeTools = new TimeTools();
    private ClassicRoom _room;
    private Camera _camera;

    private int _timeBetween2Screenshots;

    #endregion


    private void Awake()
    {
        _camera = Camera.main;
        if (!Directory.Exists(MainMenuController.PresetData == null
                ? Application.dataPath
                : MainMenuController.PresetData.ExportPath == null
                    ? Application.dataPath
                    : MainMenuController.PresetData.ExportPath))
            Directory.CreateDirectory(MainMenuController.PresetData?.ExportPath == null
                ? Application.dataPath
                : MainMenuController.PresetData.ExportPath);
    }


    public void Init(ClassicRoom room)
    {
        _room = room;
        _random = new Random(_room.DatabaseSeed);
        _emptyQuadNodesCenters = room.EmptyQuadNodesCenters;
        
        string path = (MainMenuController.PresetData == null
            ? Application.dataPath
            : MainMenuController.PresetData.ExportPath == null
                ? Application.dataPath
                : MainMenuController.PresetData.ExportPath) + "/Export_ISGT/";

        if (!Directory.Exists(path + "OpeningsData"))
        {
            Directory.CreateDirectory(path + "OpeningsData");
        }

        _openingsDataFolderPath = path + "OpeningsData";
        _timeTools = new TimeTools();
        DatabaseGenerationData = room.DatabaseGenerationData;
    }

    /// <summary>
    /// Collects a specific amount (chosen by the user) of screenshots and openings data per room for every room.
    /// </summary>
    /// <returns></returns>
    public IEnumerator DatabaseGeneration()
    {
        _random = new Random(_room.DatabaseSeed);
        InGameMenuController.ProgressBar.value = 0;
        InGameMenuController.ProgressLabel.text = "Room_" + _room.Id;
      
        for (int j = 0; j < DatabaseGenerationData.ScreenshotsNumberPerRoom; j++)
        {
            _timeTools.Start();
            RoomsGenerator.ScreenshotsIndex++;
            TakeScreenshots(_room.RoomObject, _room.Id, j);
            InGameMenuController.ScreenshotValueLabel.text = RoomsGenerator.ScreenshotsIndex + " / " +
                                                             DatabaseGenerationData.ScreenshotsNumberPerRoom *
                                                             RoomsGenerator.NumberOfRoomToGenerate;
            yield return new WaitForSecondsRealtime(0.05f);
            RoomsGenerator.TimeBetween2Screenshots = _timeTools.GetElapsedTimeInSeconds();
        }
        _camera.transform.rotation = Quaternion.identity;
        _room.RoomState = RoomState.DatabaseGenerated;
    }
    
    /// <summary>
    /// Takes a screenshot and calls the openings data getting method.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="roomID"></param>  
    /// <param name="roomIndex"></param>
    /// <param name="screenshotIndex"></param>
    /// <returns></returns>
    private void TakeScreenshots(GameObject room, string roomID, int screenshotIndex)
    {
        InGameMenuController.ProgressBar.value =
        RoomsGenerator.ScreenshotsIndex * 100f /
                    (DatabaseGenerationData.ScreenshotsNumberPerRoom * RoomsGenerator.NumberOfRoomToGenerate);
        InGameMenuController.ProgressBar.title = "Progress: " +
                                                 InGameMenuController.ProgressBar.value +
                                                 " /  " + InGameMenuController.ProgressBar.highValue + "%";
        Camera camera = _camera!;
        camera.transform.position = RandomCameraPosition(room);
        camera.transform.rotation = RandomRotation();


        string path = (MainMenuController.PresetData == null
            ? Application.dataPath
            : MainMenuController.PresetData.ExportPath == null
                ? Application.dataPath
                : MainMenuController.PresetData.ExportPath) + "/Export_ISGT/";
        
        if (!Directory.Exists(path + "Photographs"))
        {
            Directory.CreateDirectory(path + "Photographs");
        }

        if (!Directory.Exists(path + "Photographs/Room-" + roomID))
        {
            Directory.CreateDirectory(path + "Photographs/Room-" + roomID);
        }

        string filename = $"{DateTime.UtcNow:yyyy-MM-ddTHH-mm-ss.fffZ}-P{screenshotIndex + 1}";
        
        Camera.main.TryGetComponent<CameraScreenshot>(out var cameraScreenshot);
        cameraScreenshot.savePath = path + "Photographs/Room-" + roomID + "/" + filename + ".png";
        cameraScreenshot.CaptureScreenshot();
        GetOpeningsData(room, screenshotIndex, filename);
    }
    
    #region Random Camera Coordinates Calculation Methods

    /// <summary>
    /// Randomly places the camera in the room volume but respecting distances (choosen by the user) from ceiling, ground, walls and objects.
    /// </summary>
    /// <param name="room"></param>
    /// <returns></returns>
    private Vector3 RandomCameraPosition(GameObject room)
    {
        Vector3 nextCameraPosition = new Vector3();
        bool positionSet;
        do
        {
            positionSet = true;
            int emptyNodeIndex = _random.Next(_emptyQuadNodesCenters.Count);
            Bounds EmptyNode = _emptyQuadNodesCenters[emptyNodeIndex];
            // Generate a random position within the ground area
            float xComponent = NextDouble(_random, EmptyNode.min.x + NextDouble(_random, 0, EmptyNode.size.x),
                EmptyNode.max.x - NextDouble(_random, 0, EmptyNode.size.x));
            float zComponent = NextDouble(_random, EmptyNode.min.z + NextDouble(_random, 0, EmptyNode.size.z),
                EmptyNode.max.z - NextDouble(_random, 0, EmptyNode.size.z));
            float yComponent = NextDouble(_random, 0.5f, 2);
            nextCameraPosition = new Vector3(xComponent, yComponent, zComponent);
            
            // Make sure the camera is not too close to obstacles like walls, doors, windows or objects
            Collider[] colliders =
                Physics.OverlapSphere(nextCameraPosition, DatabaseGenerationData.CameraMinimumDistanceFromWall);
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Walls") || collider.CompareTag("Door") || collider.CompareTag("SimObjPhysics"))
                {
                    positionSet = false;
                    break;
                }
            }
        } while (!positionSet);

        return nextCameraPosition;
    }

    private float NextDouble(Random random, float minValue, float maxValue)
    {
        return (float)(random.NextDouble() * (maxValue - minValue) + minValue);
    }

    /// <summary>
    /// Gives a random rotation for the camera.
    /// </summary>
    /// <returns></returns>
    private Quaternion RandomRotation()
    {
        float xRotation = NextDouble(_random, -DatabaseGenerationData.MaximumCameraXRotation,
            DatabaseGenerationData.MaximumCameraXRotation);
        float yRotation = NextDouble(_random, -DatabaseGenerationData.MaximumCameraYRotation,
            DatabaseGenerationData.MaximumCameraYRotation);
        float zRotation = NextDouble(_random, -DatabaseGenerationData.MaximumCameraZRotation,
            DatabaseGenerationData.MaximumCameraZRotation);

        Vector3 rotation3D = new Vector3(xRotation, yRotation, zRotation);

        return Quaternion.Euler(rotation3D);
    }

    #endregion

    #region Data Collection And Management Methods

    /// <summary>
    /// Gets all the openings data (3 opening dimensions, distance to camera, openess (true/false), type (door/window),
    /// visibility ratio (between 0 and 1) and the 2D bounding box (origin and 2D dimensions in pixels)) for every opening visible
    /// on the corresponding screenshot.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="screenshotIndex"></param>
    /// <param name="filename"></param>
    /// <returns></returns>
    private void GetOpeningsData(GameObject room, int screenshotIndex, string filename)
    {
        TimeTools timeTools = new TimeTools();
        timeTools.Start();
        List<GameObject> walls = _room.RoomGrid.GetAllWalls();
        var cameraTransform = _camera.transform;
        ScreenshotData screenshotData = new ScreenshotData
        {
            CameraRotation = cameraTransform.rotation
        };

        Vector3 cameraPosition = cameraTransform.position;

        foreach (GameObject wall in walls)
        {
            foreach (Transform wallChild in wall.transform)
            {
                if (wallChild.TryGetComponent(out Opening opening) && opening.IsOnScreen())
                {
                    // Collect data for each opening
                    OpeningData openingData = new OpeningData
                    {
                        DistanceToCamera = (opening.GetCenter() - cameraPosition).magnitude,
                        RotationQuaternionFromCamera = Quaternion.LookRotation(opening.GetCenter() - cameraPosition),
                        OpenessDegree = opening.OpennessDegree,
                        Type = opening.Type.ToString(),
                    };

                    if (wallChild.TryGetComponent<BoxCollider>(out BoxCollider boxCollider))
                    {
                        float width = RoomsGenerator.GetOpeningWidth(boxCollider.size);
                        float height = boxCollider.size.y;
                        float thickness = Mathf.Approximately(width, boxCollider.size.x)
                            ? boxCollider.size.z
                            : boxCollider.size.x;

                        openingData.Dimensions.Add("Height", height);
                        openingData.Dimensions.Add("Width", width);
                        openingData.Dimensions.Add("Thickness", thickness);
                    }

                    openingData.BoundingBox = opening.GetFullBoundingBox();
                    openingData.VisibilityBoundingBox = opening.GetVisibilityBoundingBox();
                    openingData.VisibilityRatio = opening.GetVisibilityRatio();
                    
                    // Only store openings data if it's visible on the screenshot
                    if (openingData.VisibilityRatio > 0f)
                    {
                        screenshotData.OpeningsData.Add(openingData);
                    }
                }
            }
        }
        StoreData(screenshotData, screenshotIndex, filename);
    }

    


    /// <summary>
    ///  Stores the openings data corresponding to the screenshot identified with roomIndex and screenshotIndex in a JSON file in a specific folder. And Store the seed used to generate the room where the opening is placed, the seed used to generate the door object, the seed used to generate the window object and the seed used to generate the database.
    ///
    /// </summary>
    /// <param name="screenshotData"></param>
    /// <param name="screenshotIndex"></param>
    /// <param name="filename"></param>
    private void StoreData(ScreenshotData screenshotData, int screenshotIndex, string filename)
    {
        ClassicRoom room = _room;
        CombinedData combinedData = new CombinedData
        {
            SeedsData = new SeedsData(room.RoomSeed, room.OpeningSeed, room.ObjectSeed, room.DatabaseSeed),
            CameraData = new CameraData(_camera.fieldOfView, _camera.nearClipPlane, _camera.farClipPlane,
                _camera.pixelRect.x, _camera.pixelRect.y, _camera.pixelWidth, _camera.pixelHeight, _camera.depth,
                _camera.orthographic),
            ScreenshotData = screenshotData
        };

        string json = JsonConvert.SerializeObject(combinedData, Formatting.Indented);

        string directoryPath = Path.Combine(_openingsDataFolderPath, $"Room-{room.Id}");
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string filePath = Path.Combine(directoryPath, filename + ".json");
        File.WriteAllText(filePath, json);
    }

    private class CombinedData
    {
        public SeedsData SeedsData { get; set; }
        public CameraData CameraData { get; set; }
        public ScreenshotData ScreenshotData { get; set; }
    }

    #endregion
}