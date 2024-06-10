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
    public static int RoomIndex = 0;
    public static int NumberOfRoomToGenerate = 0; 
    public static int ScreenshotsIndex = 0;

    public static float TimeBetween2Screenshots = 0;
    #endregion

    #region Private Fields

    private Room _room;
    [HideInInspector] [SerializeField] private int _roomSeed;
    [HideInInspector] [SerializeField] private int _openingSeed;
    [HideInInspector] [SerializeField] private int _objectSeed;
    [HideInInspector] [SerializeField] private int _databaseSeed;
    TimeTools _timeTools = new TimeTools();
    TimeTools _timeTools2 = new TimeTools();
    #endregion

    #region Methods Called By Buttons

    private void Awake()
    {
        if (MainMenuController.PresetData != null)
        {
            RoomsGenerationData.MaxRoomWidth = MainMenuController.PresetData.MaxWidth;
            RoomsGenerationData.MaxRoomHeight = MainMenuController.PresetData.MaxDepth;
            RoomsGenerationData.ObjectNumberRatio = MainMenuController.PresetData.PropsRatio;
            RoomsGenerationData.WindowPerWallNumber = MainMenuController.PresetData.WindowRatio;
            RoomsGenerationData.DoorPerWallNumber = MainMenuController.PresetData.DoorRatio;
            RoomsGenerationData.NumberOfEmptyRoomsOnScene = MainMenuController.PresetData.NumberOfRoomsToGenerate;
            DatabaseGenerationData.ScreenshotsNumberPerRoom = MainMenuController.PresetData.ScreenshotsCountPerRoom;
            NumberOfRoomToGenerate = MainMenuController.PresetData.NumberOfRoomsToGenerate;
        }

        NumberOfRoomToGenerate = RoomsGenerationData.NumberOfEmptyRoomsOnScene;
    }

    private void Start()
    {
        StartCoroutine(GenerateRooms());
        _timeTools2.Start();
    }

    private void Update()
    {
        if (_timeTools != null && _room != null)
        {
           // actualise l'éta toute les 2 sec pour ne pas surcharger le calcul
            if (_timeTools2.GetElapsedTimeInSeconds() >=0.99f)
            {
                InGameMenuController.ElapsedTimeValueLabel.text = _timeTools.GetStringFormattedElapsedTime();
                InGameMenuController.ETAValueLabel.text = FormattedRemainingTime();
                _timeTools2.Start();
            }
          
        }
    }
    private string FormattedRemainingTime()
    {
        float remainingTime =TimeBetween2Screenshots * (DatabaseGenerationData.ScreenshotsNumberPerRoom * NumberOfRoomToGenerate - ScreenshotsIndex);
        int hours = (int)remainingTime / 3600;
        int minutes = (int)(remainingTime % 3600) / 60;
        int seconds = (int)(remainingTime % 3600) % 60;
        if (hours > 0)
            return hours + "h " + minutes + "m " + seconds + "s";
        if (minutes > 0)
            return minutes + "m " + seconds + "s";
        
        return seconds + "s";
    }


    /// <summary>
    /// This method calls all the methods building the final rooms without painting on surfaces. 
    /// (Creation of the room shells, then the openings in the walls and finally the objects).
    /// </summary>
    private IEnumerator GenerateRooms()
    {
        Debug.Log("Generating Rooms");
        _timeTools.Start();
        for (int i = 0; i < RoomsGenerationData.NumberOfEmptyRoomsOnScene; i++)
        {
            RoomIndex= i + 1;
            InGameMenuController.RoomValueLabel.text =  RoomIndex + " / " + RoomsGenerationData.NumberOfEmptyRoomsOnScene;
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

        _timeTools.Stop();
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
        for (int i = 0; i < room.transform.childCount; i++)
        {
            RoomCategory roomCategory;

            if (Enum.TryParse(room.transform.GetChild(i).gameObject.name, out roomCategory) &&
                roomCategory == wantedCategory)
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