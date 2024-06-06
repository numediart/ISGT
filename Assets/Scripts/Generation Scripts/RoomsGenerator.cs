using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.SceneManagement;
using Utils;

public class RoomsGenerator : MonoBehaviour
{
    #region Public Fields

    public RoomsGenerationScriptableObject RoomsGenerationData;
    public DatabaseGenerationScriptableObject DatabaseGenerationData;
    public bool _manualSeeds = false;
    
    #endregion

    #region Private Fields
    private Room _room;
    [HideInInspector] [SerializeField] private int _roomSeed;
    [HideInInspector] [SerializeField] private int _openingSeed;
    [HideInInspector] [SerializeField] private int _objectSeed;
    [HideInInspector] [SerializeField] private int _databaseSeed;
    
    #endregion

    #region Methods Called By Buttons

    private void Awake()
    {
        RoomsGenerationData.MaxRoomWidth = MainMenuController.PresetData.MaxWidth;
        RoomsGenerationData.MaxRoomHeight = MainMenuController.PresetData.MaxDepth;
        RoomsGenerationData.ObjectNumberRatio = MainMenuController.PresetData.PropsRatio;
        RoomsGenerationData.WindowPerWallNumber = MainMenuController.PresetData.WindowRatio;
        RoomsGenerationData.DoorPerWallNumber = MainMenuController.PresetData.DoorRatio;
        RoomsGenerationData.NumberOfEmptyRoomsOnScene = MainMenuController.PresetData.NumberOfRoomsToGenerate;
        DatabaseGenerationData.ScreenshotsNumberPerRoom = MainMenuController.PresetData.ScreenshotsCountPerRoom;
    }

    private void Start()
    {
        StartCoroutine(GenerateRooms());
    }

    /// <summary>
    /// This method calls all the methods building the final rooms without painting on surfaces. 
    /// (Creation of the room shells, then the openings in the walls and finally the objects).
    /// </summary>
    private IEnumerator GenerateRooms()
    {
        Debug.Log(RoomsGenerationData.NumberOfEmptyRoomsOnScene);
        TimeTools timeTools = new TimeTools();
        timeTools.Start();
        for (int i = 0; i < RoomsGenerationData.NumberOfEmptyRoomsOnScene; i++)
        {
            GameObject go = new GameObject("GeneratedRoom");
            go.AddComponent<Room>();
            go.TryGetComponent<Room>(out _room);
            if (_manualSeeds)
            {
                _room.SetSeeds(_roomSeed, _openingSeed, _objectSeed, _databaseSeed);
            }

            _room.ManualSeeds = _manualSeeds;
            _room.InitRoom(RoomsGenerationData, DatabaseGenerationData);
    
            yield return new WaitUntil(() =>
            {
                if (_room.RoomState == RoomState.DatabaseGenerated)
                {
                    DestroyImmediate(go);
                    return true;
                }
                return false;
                
            });
        }
        timeTools.Stop();
        Debug.Log("Rooms generated in " + timeTools.GetElapsedTimeInSeconds() + " seconds.");
        timeTools.GetFormattedElapsedTime();
        SceneManager.LoadScene(0);
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
        var localScale = wall.transform.localScale;
        return Mathf.Max(localScale.x, localScale.z);
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
    public static bool IsCameraInsideAWall(Room room, Vector3 nextCameraPosition)
    {
        List<GameObject> walls = room.RoomGrid.GetAllWalls();

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

    public static GameObject GetRoomCategory(GameObject room, RoomCategory wantedCategory)
    {
        for(int i = 0; i < room.transform.childCount; i++)
        {
            RoomCategory roomCategory;

            if (Enum.TryParse(room.transform.GetChild(i).gameObject.name, out roomCategory) && roomCategory == wantedCategory)
                return room.transform.GetChild(i).gameObject;
        }

        return null;
    }
}


public enum RoomCategory
{
    Grounds,
    Ceiling,
    Walls,
    Objects
}