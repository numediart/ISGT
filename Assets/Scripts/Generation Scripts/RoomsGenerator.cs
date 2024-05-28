using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class RoomsGenerator : MonoBehaviour
{
    #region Public Fields

    public RoomsGenerationScriptableObject RoomsGenerationData;
    private Room _room;

    #endregion

    #region Private Fields

    public bool _manualSeeds = false;
    [HideInInspector] [SerializeField] private int _roomSeed;
    [HideInInspector] [SerializeField] private int _openingSeed;
    [HideInInspector] [SerializeField] private int _objectSeed;
    [HideInInspector] [SerializeField] private int _databaseSeed;

    #endregion
    
    #region Methods Called By Buttons

    private void Start()
    {
        GenerateRooms();
    }

    /// <summary>
    /// This method calls all the methods building the final rooms without painting on surfaces. 
    /// (Creation of the room shells, then the openings in the walls and finally the objects).
    /// </summary>
    private void GenerateRooms()
    {
        GameObject go = new GameObject("GeneratedRoom");
         go.AddComponent<Room>();
         go.TryGetComponent<Room>(out _room);
        if (_manualSeeds)
        {
            _room.SetSeeds(_roomSeed, _openingSeed, _objectSeed, _databaseSeed);
        }
        _room.ManualSeeds = _manualSeeds;
        Debug.Log("Room Generation Started");
        _room.InitRoom(RoomsGenerationData);
        Debug.Log("Room Generation Finished");
    }

    /// <summary>
    /// Destroy all the rooms created in the unity scene.
    /// </summary>
    public void ClearScene()
    {
        if (_room != null)
        {
            DestroyImmediate(_room.gameObject);
        }
    }

    #endregion
    

    #region Information And Objects Getting Methods

    public static float GetOpeningWidth(Vector3 colliderSize)
    {
        return Mathf.Max(colliderSize.x, colliderSize.z);
    }

    public float GetWallWidth(GameObject wall)
    {
        return Mathf.Max(wall.transform.localScale.x, wall.transform.localScale.z);
    }
    
    public static List<GameObject> GetRoomCategoryObjects(GameObject room, RoomCategory category)
    {
        List<GameObject> categoryObjects = new List<GameObject>();
        foreach (Transform child in room.transform)
        {
            if (child.CompareTag(category.ToString()))
            {
                categoryObjects.Add(child.gameObject);
            }
        }

        return categoryObjects;
    }

    /// <summary>
    /// Verifies if the camera position is located inside one of the walls of a given room.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="nextCameraPosition"></param>
    /// <returns></returns>
    public static bool IsCameraInsideAWall(GameObject room, Vector3 nextCameraPosition)
    {
        List<GameObject> walls = GetRoomCategoryObjects(room, RoomCategory.Walls);

        if (walls.Count == 0) return false;

        Vector3 genericWallDimensions = (walls[0].transform.childCount == 0)
            ? walls[0].transform.localScale
            : walls[0].transform.GetChild(0).localScale;

        float wallThickness = Mathf.Min(genericWallDimensions.x, genericWallDimensions.z);

        foreach (GameObject wall in walls)
        {
            GameObject genericWall = (wall.transform.childCount == 0) ? wall : wall.transform.GetChild(0).gameObject;
            Vector3 wallToCamera = nextCameraPosition - genericWall.transform.position;
            float forwardDistanceToWall = Vector3.Project(wallToCamera, -genericWall.transform.forward).magnitude;
            float sideDistanceToWall = Vector3.Project(wallToCamera, genericWall.transform.right).magnitude;

            float wallWidth = Mathf.Max(genericWallDimensions.x, genericWallDimensions.z);

            if (forwardDistanceToWall <= wallThickness / 2f && sideDistanceToWall < wallWidth / 2f)
                return true;
        }

        return false;
    }

    #endregion
}

public enum RoomCategory
{
    Grounds,
    Ceiling,
    Walls,
    Objects
}
