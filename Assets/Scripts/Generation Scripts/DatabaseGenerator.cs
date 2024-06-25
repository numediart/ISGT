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
    public bool manualScreenShots = false;

    #endregion

    #region Private Fields

    private float _timeBetweenScreenshots;
    private string _openingsDataFolderPath;
    private Random _random;
    [SerializeField] [HideInInspector] private List<Vector3> _cameraPositions;
    [SerializeField] [HideInInspector] private List<Vector3> _cameraRotations;
    private int _cameraIndex = 0;
    private List<Bounds> _emptyQuadNodesCenters;
    private TimeTools _timeTools = new TimeTools();
    private Room _room;
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


    public void Init(Room room)
    {
        _room = room;
        _random = new Random(_room.DatabaseSeed);
        this._emptyQuadNodesCenters = room.EmptyQuadNodesCenters;


        string path = (MainMenuController.PresetData == null
            ? Application.dataPath
            : MainMenuController.PresetData.ExportPath == null
                ? Application.dataPath
                : MainMenuController.PresetData.ExportPath) + "/";
        Debug.Log("Export path: " + path);

        if (!Directory.Exists(path + "OpeningsData"))
        {
            Directory.CreateDirectory(path + "OpeningsData");
        }

        _openingsDataFolderPath = path + "OpeningsData";
        _timeTools = new TimeTools();

        DatabaseGenerationData = room.DatabaseGenerationData;
    }

    /// <summary>
    /// Collects a specific amount (choosen by the user) of screenshots and openings data per room for every room.
    /// </summary>
    /// <returns></returns>
    public IEnumerator DatabaseGeneration()
    {
        _random = new Random(_room.DatabaseSeed);
        InGameMenuController.ProgressBar.value = 0;
        InGameMenuController.ProgressLabel.text = "Room_" + _room.Id;
        if (manualScreenShots)
        {
            //Iterate over cam positions and rotations
            for (int i = 0; i < _cameraPositions.Count; i++)
            {
                _timeTools.Start();
                RoomsGenerator.ScreenshotsIndex++;

                TakeScreenshots(_room.RoomObject, _room.Id, i);
                InGameMenuController.ScreenshotValueLabel.text = RoomsGenerator.ScreenshotsIndex + " / " +
                                                                 DatabaseGenerationData.ScreenshotsNumberPerRoom *
                                                                 RoomsGenerator.NumberOfRoomToGenerate;
                yield return new WaitForSeconds(DatabaseGenerationData.TimeBetweenScreenshotsInManualMode);
                RoomsGenerator.TimeBetween2Screenshots = _timeTools.GetElapsedTime();
            }
        }
        else
        {
            for (int j = 0; j < DatabaseGenerationData.ScreenshotsNumberPerRoom; j++)
            {
                _timeTools.Start();
                RoomsGenerator.ScreenshotsIndex++;
                TakeScreenshots(_room.RoomObject, _room.Id, j);
                InGameMenuController.ScreenshotValueLabel.text = RoomsGenerator.ScreenshotsIndex + " / " +
                                                                 DatabaseGenerationData.ScreenshotsNumberPerRoom *
                                                                 RoomsGenerator.NumberOfRoomToGenerate;
                yield return new WaitForSecondsRealtime(0.05f);
                // yield return WaitForSpacebarPress();
                RoomsGenerator.TimeBetween2Screenshots = _timeTools.GetElapsedTimeInSeconds();
            }
        }


        _camera.transform.rotation = Quaternion.identity;


        _room.RoomState = RoomState.DatabaseGenerated;
    }
    
    private IEnumerator WaitForSpacebarPress()
    {
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }
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
        manualScreenShots = false;
        camera.transform.position = RandomCameraPosition(room);
        camera.transform.rotation =
            manualScreenShots ? Quaternion.Euler(_cameraRotations[_cameraIndex]) : RandomRotation();
        _cameraIndex++;


        string path = (MainMenuController.PresetData == null
            ? Application.dataPath
            : MainMenuController.PresetData.ExportPath == null
                ? Application.dataPath
                : MainMenuController.PresetData.ExportPath) + "/";
        //Camera.main.Render();
        // You need to comment the line below if you want to use the camera stereo mode and take a screenshot with each eye.
        if (!Directory.Exists(path + "Photographs"))
        {
            Directory.CreateDirectory(path + "Photographs");
        }

        if (!Directory.Exists(path + "Photographs/Room-" + roomID))
        {
            Directory.CreateDirectory(path + "Photographs/Room-" + roomID);
        }

        string filename = $"{DateTime.UtcNow:yyyy-MM-ddTHH-mm-ss.fffZ}-P{screenshotIndex + 1}";

        //  yield return new WaitForSecondsRealtime(0.01f);
      // ScreenCapture.CaptureScreenshot(
        //    $"{path}Photographs/Room-{roomID}/" + filename + ".png", 1);
        
        Camera.main.TryGetComponent<CameraScreenshot>(out var cameraScreenshot);
        cameraScreenshot.savePath = path + "Photographs/Room-" + roomID + "/" + filename + ".png";
        cameraScreenshot.CaptureScreenshot();
        
        GetOpeningsData(room, screenshotIndex, filename);
    }


// You need to uncomment the lines below to take a screenshot with each eye from a view point if tou use the _cameraera stereo mode.
/*        ScreenCapture.CaptureScreenshot($"Photographs/Room{roomIndex + 1}-P{screenshotIndex + 1}-LL.png", ScreenCapture.StereoScreenCaptureMode.LeftEye);

        yield return new WaitForSeconds(0.1f);

        ScreenCapture.CaptureScreenshot($"Photographs/Room{roomIndex + 1}-P{screenshotIndex + 1}-RL.png", ScreenCapture.StereoScreenCaptureMode.RightEye);#1#

        yield return new WaitForSeconds(DatabaseGenerationData.TimeBetweenScreenshotAndDataGetting);
    }

    #endregion
*/


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


        //  Debug.LogError("Error - Grounds child object not found or empty :\n" + ex);
        //    nextCameraPosition = Vector3.negativeInfinity;

        return nextCameraPosition;
    }

    public float NextDouble(Random random, float minValue, float maxValue)
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
    /// <param name="roomIndex"></param>
    /// <param name="screenshotIndex"></param>
    /// <returns></returns>
    private void GetOpeningsData(GameObject room, int screenshotIndex, string filename)
    {
        Debug.LogError("GetOpeningsData");
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
                    Debug.Log("Opening " + opening.gameObject.name + " is visible");
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

                    openingData.BoundingBox = GetOpeningBoundingBox2D(wallChild.gameObject);
                    openingData.VisibilityBoundingBox = opening.GetVisibilityBoundingBox();
                    openingData.VisibilityRatio = opening.GetVisibilityRatio();
                    
                    Debug.Log("Visibility ratio: " + openingData.VisibilityRatio);
                    
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
    /// Get the bounding box 2D (origin and 2D dimensions) in pixels of a given opening on a screenshot.
    /// </summary>
    /// <param name="opening"></param>
    /// <returns></returns>
    private BoundingBox2D GetOpeningBoundingBox2D(GameObject opening)
    {
        if (!opening.TryGetComponent<Opening>(out Opening openingComponent) ||
            !opening.TryGetComponent<BoxCollider>(out BoxCollider boxCollider))
        {
            return null;
        }

        Vector3 openingPosition = openingComponent.GetCenter();
        Vector3 colliderSize = boxCollider.size;
        float width = RoomsGenerator.GetOpeningWidth(colliderSize);
        float height = colliderSize.y;

        Vector3[] corners = new Vector3[4];
        var right = opening.transform.right;
        var up = opening.transform.up;
        corners[0] = openingPosition - right * width / 2f - up * height / 2f;
        corners[1] = openingPosition + right * width / 2f - up * height / 2f;
        corners[2] = openingPosition + right * width / 2f + up * height / 2f;
        corners[3] = openingPosition - right * width / 2f + up * height / 2f;

        Vector3[] screenCorners = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            screenCorners[i] = _camera.WorldToScreenPoint(corners[i]);
            if (screenCorners[i].z < 0)
            {
                Vector3 distVector =
                    Vector3.Project(
                        _camera.transform.position + _camera.transform.forward * _camera.nearClipPlane - corners[i],
                        _camera.transform.forward);
                screenCorners[i] = _camera.WorldToScreenPoint(corners[i] + distVector);
            }
        }

        float minX = Mathf.Min(screenCorners[0].x, screenCorners[1].x, screenCorners[2].x, screenCorners[3].x);
        float minY = Mathf.Min(screenCorners[0].y, screenCorners[1].y, screenCorners[2].y, screenCorners[3].y);
        float maxX = Mathf.Max(screenCorners[0].x, screenCorners[1].x, screenCorners[2].x, screenCorners[3].x);
        float maxY = Mathf.Max(screenCorners[0].y, screenCorners[1].y, screenCorners[2].y, screenCorners[3].y);

        int screenWidth = _camera.pixelWidth;
        int screenHeight = _camera.pixelHeight;

        if (minX > screenWidth || minY > screenHeight || maxX < 0 || maxY < 0)
        {
            return null;
        }

        Vector2Int boundingBoxOrigin =
            new Vector2Int(Mathf.Clamp((int)minX, 0, screenWidth), Mathf.Clamp((int)minY, 0, screenHeight));
        int boxWidth = Mathf.Clamp((int)maxX, 0, screenWidth) - boundingBoxOrigin.x;
        int boxHeight = Mathf.Clamp((int)maxY, 0, screenHeight) - boundingBoxOrigin.y;
        
        
        int screenShotWidth = MainMenuController.PresetData.ImageWidth;
        int screenShotHeight = MainMenuController.PresetData.ImageHeight;
        
        // Scale coordinates to screenshot size
        boundingBoxOrigin = new Vector2Int((int)(boundingBoxOrigin.x * screenShotWidth / screenWidth),
            (int)(boundingBoxOrigin.y * screenShotHeight / screenHeight));
        boxWidth = (int)(boxWidth * screenShotWidth / screenWidth);
        boxHeight = (int)(boxHeight * screenShotHeight / screenHeight);

        return new BoundingBox2D(boundingBoxOrigin, boxWidth, boxHeight);
    }


    /// <summary>
    ///  Stores the openings data corresponding to the screenshot identified with roomIndex and screenshotIndex in a JSON file in a specific folder. And Store the seed used to generate the room where the opening is placed, the seed used to generate the door object, the seed used to generate the window object and the seed used to generate the database.
    ///
    /// </summary>
    /// <param name="screenshotData"></param>
    /// <param name="screenshotIndex"></param>
    private void StoreData(ScreenshotData screenshotData, int screenshotIndex, string filename)
    {
        Room room = _room;
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

    public class CombinedData
    {
        public SeedsData SeedsData { get; set; }
        public CameraData CameraData { get; set; }
        public ScreenshotData ScreenshotData { get; set; }
    }

    #endregion
}