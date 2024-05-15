using System;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Data_Classes;
using UnityEditor;
using Utils;
using Random = System.Random;

public class DatabaseGenerator : MonoBehaviour
{
    #region Public Fields

    public ObjectsGenerationScriptableObject ObjectsGenerationData;
    public DatabaseGenerationScriptableObject DatabaseGenerationData;
    public GeneratorsContainer GeneratorsContainer;
    public bool manualScreenShots;
    #endregion

    #region Private Fields

    private float _timeBetweenScreenshots;
    private string _openingsDataFolderPath;
    private Random _random;
    [SerializeField] [HideInInspector] private List<Vector3> _cameraPositions;
    [SerializeField] [HideInInspector] private List<Vector3> _cameraRotations;
    private int _cameraIndex = 0;

    #endregion

    private void Start()
    {
        Camera.main.Render();
        string path = Directory.GetCurrentDirectory();
        if (!Directory.Exists(path + "/OpeningsData"))
        {
            Directory.CreateDirectory(path + "/OpeningsData");
        }

        _openingsDataFolderPath = path + "/OpeningsData";
        StartCoroutine(DatabaseGeneration());
    }

    #region Database Generation Methods

    /// <summary>
    /// Collects a specific amount (choosen by the user) of screenshots and openings data per room for every room.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DatabaseGeneration()
    {
        if (manualScreenShots)
        {
            //Iterate over cam positions and rotations
            for (int i = 0; i < _cameraPositions.Count; i++)
            {
                Debug.Log("Taking screenshot " + i);
                KeyValuePair<int, Room> room = RoomsGenerator.RoomsDictionary.ElementAt(0);
                StartCoroutine(TakeScreenshots(GameObject.Find("Room_" + room.Key), room.Value.Id, room.Key, i));
                yield return new WaitForSeconds(DatabaseGenerationData.TimeBetweenScreenshotsInManualMode);
            }
        }
        else
        {
            //GeneratorsContainer.RoomsGenerator.GenerateRooms();
            foreach (KeyValuePair<int, Room> room in RoomsGenerator.RoomsDictionary)
            {
                _random = room.Value.DatabaseRandom;
                for (int j = 0; j < DatabaseGenerationData.ScreenshotsNumberPerRoom; j++)
                {
                
                    StartCoroutine(TakeScreenshots(GameObject.Find("Room_"+room.Key), room.Value.Id, room.Key, j));
                    yield return new WaitForSeconds(0.25f);
                }
            }

            Camera.main.transform.position = new Vector3(0, 100, 0);
            Camera.main.transform.rotation = Quaternion.identity;
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
    private IEnumerator TakeScreenshots(GameObject room, string roomID, int roomIndex, int screenshotIndex)
    {
        Camera camera = Camera.main;
        Transform currentCameraTransform = camera.transform;
        camera.transform.position = manualScreenShots ? _cameraPositions[_cameraIndex] : RandomCameraPosition(room);
        camera.transform.rotation = manualScreenShots ? Quaternion.Euler(_cameraRotations[_cameraIndex]) : RandomRotation();
        _cameraIndex++;
        //Camera.main.Render();
        Debug.Log("Camera position: " + camera.transform.position);
        yield return new WaitWhile(() => !currentCameraTransform.position.Equals(camera.transform.position));

        // You need to comment the line below if you want to use the camera stereo mode and take a screenshot with each eye.
        if (!Directory.Exists("Photographs"))
        {
            Directory.CreateDirectory("Photographs");
        }

        if (!Directory.Exists("Photographs/Room-" + roomID))
        {
            Debug.Log("Creating directory Room-" + roomID + " for photographs.");
            Directory.CreateDirectory("Photographs/Room-" + roomID);
            Debug.Log("Directory created.");
        }

       Debug.Log("Taking screenshot " + screenshotIndex);
            ScreenCapture.CaptureScreenshot(
                $"Photographs/Room-{roomID}/{DateTime.UtcNow:yyyy-MM-ddTHH-mm-ss.fffZ}-P{screenshotIndex + 1}.png", 4);
      
        // You need to uncomment the lines below to take a screenshot with each eye from a view point if tou use the camera stereo mode.
/*        ScreenCapture.CaptureScreenshot($"Photographs/Room{roomIndex + 1}-P{screenshotIndex + 1}-LL.png", ScreenCapture.StereoScreenCaptureMode.LeftEye);

        yield return new WaitForSeconds(0.1f);

        ScreenCapture.CaptureScreenshot($"Photographs/Room{roomIndex + 1}-P{screenshotIndex + 1}-RL.png", ScreenCapture.StereoScreenCaptureMode.RightEye);*/

        yield return new WaitForSeconds(DatabaseGenerationData.TimeBetweenScreenshotAndDataGetting);
        GetOpeningsData(room, roomIndex, screenshotIndex);
    }

    #endregion

    #region Random Camera Coordinates Calculation Methods

    /// <summary>
    /// Randomly places the camera in the room volume but respecting distances (choosen by the user) from ceiling, ground, walls and objects.
    /// </summary>
    /// <param name="room"></param>
    /// <returns></returns>
   private Vector3 RandomCameraPosition(GameObject room)
{
    GameObject grounds = RoomsGenerator.GetRoomCategory(room, RoomCategory.Grounds);
    GameObject walls = RoomsGenerator.GetRoomCategory(room, RoomCategory.Walls);
    float ceilingHeight = walls.transform.GetChild(0).transform.localScale.y;
    Vector3 nextCameraPosition;

    try
    {
        GameObject choosenGround = grounds.transform.GetChild(_random.Next(0, grounds.transform.childCount)).gameObject;

        float scaleMultiplier = (choosenGround.transform.childCount > 0) ? 1f : 10f;

        Transform groundTransform = choosenGround.transform;
        MeshCollider meshCollider = null;

        if (choosenGround.transform.childCount > 0)
        {
            meshCollider = RoomsGenerator.GetBrushesFirstMeshCollider(choosenGround).GetComponent<MeshCollider>();
        }

        Vector3 choosenGroundPosition = meshCollider != null ? meshCollider.bounds.center : groundTransform.position;
        Vector3 choosenGroundScale = meshCollider != null ? meshCollider.bounds.size : groundTransform.localScale;

        bool positionSet = false;

        do
        {
            positionSet = true;

            // Generate a random position within the ground area
            float xComponent = NextDouble(_random,
                -choosenGroundScale.x * scaleMultiplier / 2f + DatabaseGenerationData.CameraMinimumDistanceFromWall,
                choosenGroundScale.x * scaleMultiplier / 2f - DatabaseGenerationData.CameraMinimumDistanceFromWall);
            float zComponent = NextDouble(_random,
                -choosenGroundScale.z * scaleMultiplier / 2f + DatabaseGenerationData.CameraMinimumDistanceFromWall,
                choosenGroundScale.z * scaleMultiplier / 2f - DatabaseGenerationData.CameraMinimumDistanceFromWall);
            float yComponent = NextDouble(_random,DatabaseGenerationData.CameraMinimumDistanceFromGroundAndCeiling,
                ceilingHeight - DatabaseGenerationData.CameraMinimumDistanceFromGroundAndCeiling);

            nextCameraPosition = choosenGroundPosition + groundTransform.right * xComponent + groundTransform.forward * zComponent + groundTransform.up * yComponent;

            Collider[] colliders = Physics.OverlapSphere(nextCameraPosition, DatabaseGenerationData.CameraMinimumDistanceFromWall);
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.layer == 0)
                {
                    positionSet = false;
                    break;
                }
            }

            if (!positionSet)
            {
                continue;
            }

            colliders = Physics.OverlapSphere(nextCameraPosition, DatabaseGenerationData.CameraMinimumDistanceFromObjects);
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.layer == ObjectsGenerationData.ObjectsLayerIndex)
                {
                    positionSet = false;
                    nextCameraPosition += (Camera.main.transform.position - collider.ClosestPoint(nextCameraPosition)).normalized * DatabaseGenerationData.CameraMinimumDistanceFromObjects;
                    break;
                }
            }

            if (positionSet)
            {
                positionSet = !GeneratorsContainer.ObjectsGenerator.IsCameraInsideAnObject(room, nextCameraPosition) &&
                              !RoomsGenerator.IsCameraInsideAWall(room, nextCameraPosition);
            }
        } while (!positionSet);

    }
    catch (Exception ex)
    {
        Debug.LogError("Error - Grounds child object not found or empty :\n" + ex);
        nextCameraPosition = Vector3.negativeInfinity;
    }

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
        float xRotation = _random.Next((int)-DatabaseGenerationData.MaximumCameraXRotation,
            (int)DatabaseGenerationData.MaximumCameraXRotation);
        float yRotation = _random.Next((int)-DatabaseGenerationData.MaximumCameraYRotation,
            (int)DatabaseGenerationData.MaximumCameraYRotation);
        float zRotation = _random.Next((int)-DatabaseGenerationData.MaximumCameraZRotation,
            (int)DatabaseGenerationData.MaximumCameraZRotation);

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
    private void GetOpeningsData(GameObject room, int roomIndex, int screenshotIndex)
    {
        GeneratorsContainer.ObjectsGenerator.EnableAndDisableObjectsBoundingBoxes(room,
            ObjectsBoundingBoxesAction.Disable);

        List<GameObject> walls = RoomsGenerator.GetRoomCategoryObjects(room, RoomCategory.Walls);

        ScreenshotData screenshotData = new ScreenshotData();
        screenshotData.CameraRotation = Camera.main.transform.rotation;

        foreach (GameObject wall in walls)
        {
            for (int i = 0; i < wall.transform.childCount; i++)
            {
                GameObject wallObject = wall.transform.GetChild(i).gameObject;

                if (wallObject.GetComponent<Opening>() && wallObject.GetComponent<Opening>().IsVisible())
                {
                    OpeningData openingData = new OpeningData();

                    openingData.Dimensions.Add("Height", wallObject.GetComponent<BoxCollider>().size.y);
                    openingData.Dimensions.Add("Width",
                        RoomsGenerator.GetOpeningWidth(wallObject.GetComponent<BoxCollider>().size));
                    float windowThickness =
                        Mathf.Approximately(RoomsGenerator.GetOpeningWidth(wallObject.GetComponent<BoxCollider>().size),
                            wallObject.GetComponent<BoxCollider>().size.x)
                            ? wallObject.GetComponent<BoxCollider>().size.z
                            : wallObject.GetComponent<BoxCollider>().size.x;
                    openingData.Dimensions.Add("Thickness", windowThickness);

                    openingData.DistanceToCamera =
                        (wallObject.transform.position - Camera.main.transform.position).magnitude;

                    // Find the rotation quaternion from the camera to the opening. If you multiply the camera rotation by this quaternion, it will "look" at the opening.
                    openingData.RotationQuaternionFromCamera = (Quaternion.Inverse(Camera.main.transform.rotation) *
                                                                wallObject.transform.rotation);


                    openingData.OpenessDegree = wallObject.GetComponent<Opening>().OpenessDegree;
                    openingData.Type = wallObject.GetComponent<Opening>().Type.ToString();


                    openingData.VisibilityRatio = wallObject.GetComponent<Opening>().GetVisibilityRatioBetter();
                    openingData.BoundingBox = GetOpeningBoundingBox2D(wallObject);
                    openingData.VisibilityBoundingBox = wallObject.GetComponent<Opening>().GetVisibilityBoundingBox();

                    if (openingData.VisibilityRatio > 0f && openingData.BoundingBox != null)
                        screenshotData.OpeningsData.Add(openingData);
                }
            }
        }

        StoreData(screenshotData, roomIndex, screenshotIndex);

        GeneratorsContainer.ObjectsGenerator.EnableAndDisableObjectsBoundingBoxes(room,
            ObjectsBoundingBoxesAction.Enable);
    }

    /// <summary>
    /// Gets the presence or not of any object on the screen thanks to the field of view angle and the screen resolution.
    /// (Method created because the boolean MeshRenderer.isVisible is not accurate enough).
    /// </summary>
    /// <param name="anyObject"></param>
    /// <returns></returns>
    public static bool IsOnScreen(GameObject anyObject)
    {
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 cameraForwardVector = Camera.main.transform.forward;
        Vector3 cameraToObject = anyObject.transform.position - cameraPosition;

        float verticalFieldOfView = Camera.main.fieldOfView;
        float horizontalFieldOfView = Camera.VerticalToHorizontalFieldOfView(verticalFieldOfView, Camera.main.aspect);

        Vector3 XZCameraToObject = Vector3.Project(cameraToObject, Camera.main.transform.right.normalized) +
                                   Vector3.Project(cameraToObject, cameraForwardVector.normalized);
        if (Mathf.Abs(Vector3.Angle(cameraForwardVector, XZCameraToObject)) > horizontalFieldOfView / 2f)
            return false;

        Vector3 ZYCameraToObject = Vector3.Project(cameraToObject, cameraForwardVector.normalized) +
                                   Vector3.Project(cameraToObject, Camera.main.transform.up.normalized);
        if (Mathf.Abs(Vector3.Angle(cameraForwardVector, ZYCameraToObject)) > verticalFieldOfView / 2f)
            return false;

        return true;
    }

    /// <summary>
    /// Get the bounding box 2D (origin and 2D dimensions) in pixels of a given opening on a screenshot.
    /// </summary>
    /// <param name="opening"></param>
    /// <returns></returns>
    private BoundingBox2D GetOpeningBoundingBox2D(GameObject opening)
    {
        Vector3 openingPosition = opening.transform.position;
        Vector3 colliderSize = opening.GetComponent<BoxCollider>().size;

        int screenWidth = Camera.main.pixelWidth;
        int screenHeight = Camera.main.pixelHeight;

        float width = RoomsGenerator.GetOpeningWidth(colliderSize);
        float height = colliderSize.y;

        Vector3 onScreenBottomLeftCorner = Camera.main.WorldToScreenPoint(
            openingPosition - opening.transform.right.normalized * width / 2f -
            opening.transform.up.normalized * height / 2f);
        if (onScreenBottomLeftCorner.z < 0)
        {
            Vector3 distVector = Vector3.Project((Camera.main.transform.position +
                                                  Camera.main.transform.forward * Camera.main.nearClipPlane
                                                  - (openingPosition - opening.transform.right.normalized * width / 2f -
                                                     opening.transform.up.normalized * height / 2f)),
                Camera.main.transform.forward);

            onScreenBottomLeftCorner = Camera.main.WorldToScreenPoint(
                openingPosition - opening.transform.right.normalized * width / 2f -
                opening.transform.up.normalized * height / 2f + distVector);
        }

        Vector3 onScreenBottomRightCorner = Camera.main.WorldToScreenPoint(
            openingPosition + opening.transform.right.normalized * width / 2f -
            opening.transform.up.normalized * height / 2f);
        if (onScreenBottomRightCorner.z < 0)
        {
            Vector3 distVector = Vector3.Project((Camera.main.transform.position +
                                                  Camera.main.transform.forward * Camera.main.nearClipPlane
                                                  - (openingPosition + opening.transform.right.normalized * width / 2f -
                                                     opening.transform.up.normalized * height / 2f)),
                Camera.main.transform.forward);

            onScreenBottomRightCorner = Camera.main.WorldToScreenPoint(
                openingPosition + opening.transform.right.normalized * width / 2f -
                opening.transform.up.normalized * height / 2f + distVector);
        }

        Vector3 onScreenTopRightCorner = Camera.main.WorldToScreenPoint(
            openingPosition + opening.transform.right.normalized * width / 2f +
            opening.transform.up.normalized * height / 2f);
        if (onScreenTopRightCorner.z < 0)
        {
            Vector3 distVector = Vector3.Project((Camera.main.transform.position +
                                                  Camera.main.transform.forward * Camera.main.nearClipPlane
                                                  - (openingPosition + opening.transform.right.normalized * width / 2f +
                                                     opening.transform.up.normalized * height / 2f)),
                Camera.main.transform.forward);

            onScreenTopRightCorner = Camera.main.WorldToScreenPoint(
                openingPosition + opening.transform.right.normalized * width / 2f +
                opening.transform.up.normalized * height / 2f + distVector);
        }

        Vector3 onScreenTopLeftCorner = Camera.main.WorldToScreenPoint(
            openingPosition - opening.transform.right.normalized * width / 2f +
            opening.transform.up.normalized * height / 2f);
        if (onScreenTopLeftCorner.z < 0)
        {
            Vector3 distVector = Vector3.Project((Camera.main.transform.position +
                                                  Camera.main.transform.forward * Camera.main.nearClipPlane
                                                  - (openingPosition - opening.transform.right.normalized * width / 2f +
                                                     opening.transform.up.normalized * height / 2f)),
                Camera.main.transform.forward);

            onScreenTopLeftCorner = Camera.main.WorldToScreenPoint(
                openingPosition - opening.transform.right.normalized * width / 2f +
                opening.transform.up.normalized * height / 2f + distVector);
        }

        Vector2Int boundingBoxOrigin = Vector2Int.zero;

        int minXComponent = (int)Mathf.Min(onScreenBottomLeftCorner.x, onScreenTopLeftCorner.x,
            onScreenBottomRightCorner.x, onScreenTopRightCorner.x);
        int minYComponent = (int)Mathf.Min(onScreenBottomLeftCorner.y, onScreenTopLeftCorner.y,
            onScreenBottomRightCorner.y, onScreenTopRightCorner.y);
        int maxXComponent = (int)Mathf.Max(onScreenBottomLeftCorner.x, onScreenTopLeftCorner.x,
            onScreenBottomRightCorner.x, onScreenTopRightCorner.x);
        int maxYComponent = (int)Mathf.Max(onScreenBottomLeftCorner.y, onScreenTopLeftCorner.y,
            onScreenBottomRightCorner.y, onScreenTopRightCorner.y);

        if (minXComponent > screenWidth || minYComponent > screenHeight || maxXComponent < 0 || maxYComponent < 0)
            return null;

        boundingBoxOrigin.x = (minXComponent < 0) ? 0 : minXComponent;
        boundingBoxOrigin.y = (minYComponent < 0) ? 0 : minYComponent;

        int boxWidth = ((maxXComponent > screenWidth) ? screenWidth : maxXComponent) - boundingBoxOrigin.x;
        int boxHeight = ((maxYComponent > screenHeight) ? screenHeight : maxYComponent) - boundingBoxOrigin.y;

        return new BoundingBox2D(boundingBoxOrigin, boxWidth, boxHeight);
    }

    /// <summary>
    /// Stores the openings data corresponding to the screenshot identified with roomIndex and screenshotIndex in a JSON file in a specific folder.
    /// </summary>
    /// <param name="screenshotData"></param>
    /// <param name="roomIndex"></param>
    /// <param name="screenshotIndex"></param>
    private void StoreOpeningsData(ScreenshotData screenshotData, int roomIndex, int screenshotIndex)
    {
        StringBuilder sb = new StringBuilder();

        string JSONresult = JsonConvert.SerializeObject(screenshotData, Formatting.Indented);

        string path = $"{_openingsDataFolderPath}/Room{roomIndex + 1}-P{screenshotIndex + 1}.json";

        File.WriteAllText(path, JSONresult);
    }

    /// <summary>
    ///  Stores the openings data corresponding to the screenshot identified with roomIndex and screenshotIndex in a JSON file in a specific folder. And Store the seed used to generate the room where the opening is placed, the seed used to generate the door object, the seed used to generate the window object and the seed used to generate the database.
    /// 
    /// </summary>
    /// <param name="screenshotData"></param>
    /// <param name="roomIndex"></param>
    /// <param name="screenshotIndex"></param>
    private void StoreData(ScreenshotData screenshotData, int roomIndex, int screenshotIndex)
    {
        Room room = RoomsGenerator.RoomsDictionary[roomIndex];
        CombinedData combinedData = new CombinedData
        {
            SeedsData = new SeedsData(room.RoomSeed, room.OpeningSeed, room.ObjectSeed, room.DatabaseSeed),
            ScreenshotData = screenshotData
        };
    
        string json = JsonConvert.SerializeObject(combinedData, Formatting.Indented);
    
        string path = $"{_openingsDataFolderPath}/Room{roomIndex + 1}-P{screenshotIndex + 1}.json";
        File.WriteAllText(path, json);
    }

    public class CombinedData
    {
        public SeedsData SeedsData { get; set; }
        public ScreenshotData ScreenshotData { get; set; }
    }


    #endregion
}