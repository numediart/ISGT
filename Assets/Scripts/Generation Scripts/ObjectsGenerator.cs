using System;
using RealtimeCSG.Components;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Utils;
using Random = UnityEngine.Random;

public class ObjectsGenerator : MonoBehaviour
{
    public ObjectsGenerationScriptableObject ObjectsGenerationData;
    public ObjectsGenerationType GenerationType;

    [SerializeField] private List<int> _usedSeeds = new List<int>();
    private SeedsProvider _seedsProvider;

    #region Seeds Management Methods

    private void InitiateSeed()
    {
        while (true)
        {
            _seedsProvider = new SeedsProvider();
            int newSeed = _seedsProvider.MainSeed;

            if (!_usedSeeds.Contains(newSeed))
            {
                UnityEngine.Random.InitState(newSeed);
                _usedSeeds.Add(newSeed);
                break;
            }
        }
    }

    #endregion

    #region Objects Generation Method

    /// <summary>
    /// The generic method for the objects generation in a room.
    /// </summary>
    /// <param name="room"></param>
    public void ObjectsGeneration(GameObject room)
    {
        if (GenerationType == ObjectsGenerationType.RandomGeneration)
            ObjectsRandomGeneration(room);
        else if (GenerationType == ObjectsGenerationType.RealisticGenerationWithAIModel)
            RealisticObjectsLayoutGeneration(room);
    }

    /// <summary>
    /// Generates random objects (among a list of prefabricated objects) at random positions in a given room.
    /// </summary>
    /// <param name="room"></param>
    public void ObjectsRandomGeneration(GameObject room)
    {
        InitiateSeed();

        Dictionary<GameObject, List<GameObject>> objectsByGround = new Dictionary<GameObject, List<GameObject>>();
        GameObject grounds = RoomsGenerator.GetRoomCategory(room, RoomCategory.Grounds);
        GameObject objects = RoomsGenerator.GetRoomCategory(room, RoomCategory.Objects);
        Vector3 objectPosition;

        int totalObjectsNumber = UnityEngine.Random.Range(ObjectsGenerationData.ObjectsMinimumNumberPerRoom,
            ObjectsGenerationData.ObjectsMaximumNumberPerRoom + 1);

        int missedAttempts = 0;

        while (totalObjectsNumber > 0)
        {
            GameObject ground = grounds.transform.GetChild(UnityEngine.Random.Range(0, grounds.transform.childCount))
                .gameObject;
            GameObject objectPrefab =
                ObjectsGenerationData.Objects[UnityEngine.Random.Range(0, ObjectsGenerationData.Objects.Count)];
            BoxCollider objectCollider = GetObjectBoundingBox(objectPrefab);
            Vector3 objectDimensions = objectCollider.size;
            Vector3 objectCenterPoint = objectCollider.center;

            float yRandomOrientation = UnityEngine.Random.Range(0f, 360f);
            Quaternion randomOrientation = new Quaternion();
            randomOrientation.eulerAngles =
                objectPrefab.transform.rotation.eulerAngles + new Vector3(0, yRandomOrientation, 0);
            GameObject temp = new GameObject();
            temp.transform.rotation = randomOrientation;

            float scaleMultiplier = (ground.transform.childCount > 0) ? 1f : 10f;

            Vector3 groundPosition = Vector3.negativeInfinity;
            Vector3 groundScale = Vector3.negativeInfinity;

            if (ground.transform.childCount == 0)
            {
                groundPosition = ground.transform.position;
                groundScale = ground.transform.localScale;
            }
            else
            {
                GameObject brushesFirstMeshCollider = RoomsGenerator.GetBrushesFirstMeshCollider(ground);

                brushesFirstMeshCollider.TryGetComponent(out MeshCollider meshCollider);
                Bounds bounds = meshCollider.bounds;
                groundPosition = bounds.center;
                groundScale = bounds.size;
            }

            int missedAttemptsForFirstObject = 0;
            bool positionSet = false;

            try
            {
                if (objectsByGround[ground].Count > 0)
                {
                    List<Vector3> possiblePositions = ObjectsPossiblePositions(room, objectsByGround[ground], ground,
                        objectCenterPoint, objectDimensions, temp.transform);

                    if (possiblePositions.Count > 0)
                    {
                        positionSet = true;
                        GameObject obj = Instantiate(objectPrefab,
                            possiblePositions[UnityEngine.Random.Range(0, possiblePositions.Count)], randomOrientation,
                            objects.transform);
                        objectsByGround[ground].Add(obj);
                    }
                    else if (grounds.transform.childCount > 1)
                    {
                        missedAttempts++;
                    }
                    else
                    {
                        missedAttempts++;
                        DestroyImmediate(temp);
                        break;
                    }

                    DestroyImmediate(temp);
                }
            }
            catch (KeyNotFoundException)
            {
                while (!positionSet)
                {
                    float xComponent = UnityEngine.Random.Range(
                        -groundScale.x * scaleMultiplier / 2f + Mathf.Min(objectDimensions.x, objectDimensions.z) / 2f,
                        groundScale.x * scaleMultiplier / 2f - Mathf.Min(objectDimensions.x, objectDimensions.z) / 2f);
                    float zComponent = UnityEngine.Random.Range(
                        -groundScale.z * scaleMultiplier / 2f + Mathf.Min(objectDimensions.x, objectDimensions.z) / 2f,
                        groundScale.z * scaleMultiplier / 2f - Mathf.Min(objectDimensions.x, objectDimensions.z) / 2f);
                    float yComponent = objectDimensions.y / 2f;

                    objectPosition = groundPosition + ground.transform.right.normalized * xComponent +
                                     ground.transform.forward.normalized * zComponent +
                                     ground.transform.up.normalized * yComponent;

                    positionSet = true;

                    if (!Physics.Raycast(objectPosition, Vector3.down, float.MaxValue))
                    {
                        positionSet = false;
                    }
                    else if (!WallsCollisionVerification(room, objectPosition, objectDimensions, temp.transform))
                    {
                        GameObject obj = Instantiate(objectPrefab, objectPosition - objectCenterPoint,
                            randomOrientation, objects.transform);
                        objectsByGround[ground] = new List<GameObject> { obj };
                    }
                    else
                    {
                        missedAttemptsForFirstObject++;

                        if (missedAttemptsForFirstObject < ObjectsGenerationData.MaximumNumberOfObjectPlacementAttempts)
                        {
                            positionSet = false;
                            continue;
                        }
                        else
                        {
                            positionSet = false;
                            missedAttempts++;
                            break;
                        }
                    }
                }

                DestroyImmediate(temp);
            }

            if (!positionSet && missedAttempts < ObjectsGenerationData.MaximumNumberOfObjectsToTryToFitIn)
                continue;
            else
                missedAttempts = 0;

            totalObjectsNumber--;
        }
    }

    /// <summary>
    /// Generates objects with specific position and rotation according to the JSON file describing the objects layout and generated by an AI model.
    /// </summary>
    /// <param name="room"></param>
    public void RealisticObjectsLayoutGeneration(GameObject room)
    {
        ObjectsLayout objectsLayout = GetObjectsLayoutFromJSON();

        Dictionary<string, GameObject> objectsAndId = new Dictionary<string, GameObject>();
        foreach (GameObject obj in ObjectsGenerationData.Objects)
            objectsAndId[obj.name] = obj;

        Transform objectsTransform = RoomsGenerator.GetRoomCategory(room, RoomCategory.Objects).transform;

        foreach (ObjectToPlace obj in objectsLayout.ObjectsToPlace)
        {
            Quaternion quat = new Quaternion();
            quat.eulerAngles = new Vector3(obj.Angles[0], obj.Angles[1], obj.Angles[2]);

            Vector3 pos = new Vector3(obj.PositionRelativeToRoomOrigin[0], obj.PositionRelativeToRoomOrigin[1],
                    obj.PositionRelativeToRoomOrigin[2])
                - GetObjectBoundingBox(objectsAndId[obj.Id]).center + room.transform.position;

            Instantiate(objectsAndId[obj.Id], pos, quat, objectsTransform);
        }
    }

    #endregion

    #region Data Getting Methods

    /// <summary>
    /// Returns all the possible positions for an object with a specific position and rotation to be placed in a room that already contains objects.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="placedObjects"></param>
    /// <param name="ground"></param>
    /// <param name="objectCenterPoint"></param>
    /// <param name="objectDimensions"></param>
    /// <param name="objectGroundForwardsAngle"></param>
    /// <returns></returns>
    private List<Vector3> ObjectsPossiblePositions(GameObject room, List<GameObject> placedObjects, GameObject ground,
        Vector3 objectCenterPoint, Vector3 objectDimensions, Transform objectTransform)
    {
        Vector3 groundPosition;
        Vector3 groundDimensions;
        int scaleMultiplier;

        if (ground.transform.childCount == 0)
        {
            scaleMultiplier = 10;

            groundPosition = ground.transform.position;
            groundDimensions = ground.transform.localScale;
        }
        else
        {
            scaleMultiplier = 1;

            GameObject brushesFirstMeshCollidermeshCollider = RoomsGenerator.GetBrushesFirstMeshCollider(ground);

            brushesFirstMeshCollidermeshCollider.TryGetComponent(out MeshCollider meshCollider);
            Bounds bounds = meshCollider.bounds;
            groundPosition = bounds.center;
            groundDimensions = bounds.size;
        }

        List<Vector3> possiblePositionOffsets = new List<Vector3>();

        for (float i = -groundDimensions.x * scaleMultiplier / 2f +
                       Mathf.Min(objectDimensions.x, objectDimensions.z) / 2f;
             i < groundDimensions.x * scaleMultiplier / 2f - Mathf.Min(objectDimensions.x, objectDimensions.z) / 2f;
             i += 0.05f)
        {
            for (float j = -groundDimensions.z * scaleMultiplier / 2f +
                           Mathf.Min(objectDimensions.x, objectDimensions.z) / 2f;
                 j < groundDimensions.z * scaleMultiplier / 2f - Mathf.Min(objectDimensions.x, objectDimensions.z) / 2f;
                 j += 0.05f)
            {
                Vector3 objectPosition = groundPosition + i * ground.transform.right.normalized +
                                         j * ground.transform.forward.normalized +
                                         (objectDimensions.y / 2f) * ground.transform.up.normalized;

                if (Physics.Raycast(objectPosition, Vector3.down, float.MaxValue) &&
                    !ObjectsOverlayVerification(placedObjects, objectDimensions, objectPosition, ground) &&
                    !WallsCollisionVerification(room, objectPosition, objectDimensions, objectTransform))
                    possiblePositionOffsets.Add(objectPosition - objectCenterPoint);
            }
        }

        return possiblePositionOffsets;
    }

    /// <summary>
    /// Verifies if there is an overlay between the objects already placed on the ground and a new object that would be placed at a specific position.
    /// </summary>
    /// <param name="placedObjects"></param>
    /// <param name="objectDimensions"></param>
    /// <param name="objectPosition"></param>
    /// <param name="ground"></param>
    /// <returns></returns>
    private bool ObjectsOverlayVerification(List<GameObject> placedObjects, Vector3 objectDimensions,
        Vector3 objectPosition, GameObject ground)
    {
        foreach (GameObject placedObject in placedObjects)
        {
            BoxCollider bc = GetObjectBoundingBox(placedObject);
            Vector3 placedObjectDimensions = bc.size;
            Vector3 placedObjectPosition = bc.center + placedObject.transform.position;

            float widthDistance = Vector3
                .Project(objectPosition - placedObjectPosition, ground.transform.right.normalized).magnitude;
            float lengthDistance = Vector3
                .Project(objectPosition - placedObjectPosition, ground.transform.forward.normalized).magnitude;

            float minimumWidthDistance =
                Vector3.Project(placedObjectDimensions, ground.transform.right.normalized).magnitude / 2f +
                Vector3.Project(objectDimensions, ground.transform.right.normalized).magnitude / 2f +
                ObjectsGenerationData.MinimumDistanceBetweenObjects;
            float minimumLengthDistance =
                Vector3.Project(placedObjectDimensions, ground.transform.forward.normalized).magnitude / 2f +
                Vector3.Project(objectDimensions, ground.transform.forward.normalized).magnitude / 2f +
                ObjectsGenerationData.MinimumDistanceBetweenObjects;

            if (widthDistance < minimumWidthDistance)
            {
                if (lengthDistance < minimumLengthDistance)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Verifies if a specific object is entirely in the given room or if it's partially outside the room or in a wall.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="objectPosition"></param>
    /// <param name="objectDimensions"></param>
    /// <returns></returns>
    private bool WallsCollisionVerification(GameObject room, Vector3 objectPosition, Vector3 objectDimensions,
        Transform objectTransform)
    {
        List<Vector3> cornersPositions = new List<Vector3>();
        List<GameObject> walls = RoomsGenerator.GetRoomCategoryObjects(room, RoomCategory.Walls);

        for (int x = -1; x <= 1; x += 2)
        {
            for (int z = -1; z <= 1; z += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    Vector3 cornerPosition = objectPosition +
                                             (x * objectDimensions.x / 2f) * objectTransform.right.normalized +
                                             (y * objectDimensions.y / 2f) *
                                             objectTransform.up.normalized + (z * objectDimensions.z / 2f) *
                                             objectTransform.forward.normalized;
                    cornersPositions.Add(cornerPosition);
                }
            }
        }

        foreach (GameObject wall in walls)
        {
            GameObject choosenWall = wall;
            float wallWidth;
            float wallThickness;
            wall.TryGetComponent(out CSGModel csgModel);
            if (csgModel)
                choosenWall = wall.transform.GetChild(0).gameObject;

            (wallThickness, wallWidth) = choosenWall.transform.localScale.x < choosenWall.transform.localScale.z
                ? (choosenWall.transform.localScale.x,
                    choosenWall.transform.localScale.z)
                : (choosenWall.transform.localScale.z, choosenWall.transform.localScale.x);

            if (Vector3.Project(objectPosition - choosenWall.transform.position, choosenWall.transform.right.normalized)
                    .magnitude < wallWidth / 2f)
            {
                foreach (Vector3 cornerPosition in cornersPositions)
                {
                    float distanceFromWall = Vector3.Project(cornerPosition - choosenWall.transform.position,
                            -choosenWall.transform.forward.normalized).magnitude *
                        Mathf.Sign(Vector3.Dot((cornerPosition - choosenWall.transform.position).normalized,
                            -choosenWall.transform.forward.normalized)) - wallThickness / 2f;

                    if (distanceFromWall < 0f)
                        return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the 3D box that defines the volume of a given object.
    /// </summary>
    /// <param name="objectPrefab"></param>
    /// <returns></returns>
    private BoxCollider GetObjectBoundingBox(GameObject objectPrefab)
    {
        for (int i = 0; i < objectPrefab.transform.childCount; i++)
        {
            if (objectPrefab.transform.GetChild(i).gameObject.name == "BoundingBox")
            {
                objectPrefab.transform.GetChild(i).gameObject.TryGetComponent(out BoxCollider prefabBoxCollider);
                return prefabBoxCollider;
            }
        }

        return null;
    }

    /// <summary>
    /// Verifies if the camera position is located inside an object or not.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="cameraPosition"></param>
    /// <returns></returns>
    public bool IsCameraInsideAnObject(GameObject room, Vector3 cameraPosition)
    {
        List<GameObject> objects = RoomsGenerator.GetRoomCategoryObjects(room, RoomCategory.Objects);

        foreach (GameObject obj in objects)
        {
            BoxCollider objBoundingBox = GetObjectBoundingBox(obj);
            Vector3 bbCenter = objBoundingBox.center;
            Vector3 bbDimensions = objBoundingBox.size;

            Vector3 objectCenterToCamera = cameraPosition - (obj.transform.position + bbCenter);

            if (Vector3.Project(objectCenterToCamera, obj.transform.right.normalized).magnitude < bbDimensions.x / 2f)
            {
                if (Vector3.Project(objectCenterToCamera, obj.transform.forward.normalized).magnitude <
                    bbDimensions.z / 2f)
                {
                    if (Vector3.Project(objectCenterToCamera, obj.transform.up.normalized).magnitude <
                        bbDimensions.y / 2f)
                        return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Translates the JSON file describing the objects layout in an data format understandable by the objects generation method.
    /// </summary>
    /// <returns></returns>
    private ObjectsLayout GetObjectsLayoutFromJSON()
    {
        string jsonContent = File.ReadAllText(ObjectsGenerationData.ObjectsLayoutJSONFilePath);

        ObjectsLayout layout = JsonConvert.DeserializeObject<ObjectsLayout>(jsonContent);

        if (layout != null)
            return layout;
        else
            return null;
    }

    #endregion

    public void EnableAndDisableObjectsBoundingBoxes(GameObject room, ObjectsBoundingBoxesAction action)
    {
        bool activate = action == ObjectsBoundingBoxesAction.Enable ? true : false;

        List<GameObject> objects = RoomsGenerator.GetRoomCategoryObjects(room, RoomCategory.Objects);

        foreach (GameObject obj in objects)
        {
            BoxCollider boundingBox = GetObjectBoundingBox(obj);
            boundingBox.enabled = activate;
        }
    }
}

public enum ObjectsBoundingBoxesAction
{
    Enable,
    Disable
}

public enum ObjectsGenerationType
{
    RandomGeneration,
    RealisticGenerationWithAIModel
}