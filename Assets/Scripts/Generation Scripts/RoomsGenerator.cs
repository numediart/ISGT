using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Utils;

public class RoomsGenerator : MonoBehaviour
{
    #region Public Fields

    public RoomsGenerationScriptableObject RoomsGenerationData;
    public DatabaseGenerationScriptableObject DatabaseGenerationData;
    public bool ManualSeeds = false;
    public static int RoomIndex = 0;
    public static int NumberOfRoomToGenerate = 0;
    public static int ScreenshotsIndex = 0;

    public static float TimeBetween2Screenshots = 0;

    #endregion

    #region Private Fields

    private ClassicRoom _room;
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
            RoomsGenerationData.NumberOfRoomsToGenerate = MainMenuController.PresetData.NumberOfRoomsToGenerate;
            DatabaseGenerationData.ScreenshotsNumberPerRoom = MainMenuController.PresetData.ScreenshotsCountPerRoom;
            NumberOfRoomToGenerate = MainMenuController.PresetData.NumberOfRoomsToGenerate;
            DatabaseGenerationData.MaximumCameraXRotation = MainMenuController.PresetData.MaxRotation.x;
            DatabaseGenerationData.MaximumCameraYRotation = MainMenuController.PresetData.MaxRotation.y;
            DatabaseGenerationData.MaximumCameraZRotation = MainMenuController.PresetData.MaxRotation.z;
            Camera cam = Camera.main;

            // Calculate the aspect ratio of the camera
            float aspectRatio = cam.aspect;

            // Convert diagonal FOV to radians
            float diagonalFOVRad = MainMenuController.PresetData.FieldOfView * Mathf.Deg2Rad;

            // Calculate the vertical FOV
            float verticalFOVRad =
                2f * Mathf.Atan(Mathf.Tan(diagonalFOVRad / 2f) / Mathf.Sqrt(1f + aspectRatio * aspectRatio));

            // Convert the vertical FOV back to degrees
            float verticalFOV = verticalFOVRad * Mathf.Rad2Deg;
            
            // Assign the vertical FOV to the camera
            cam.fieldOfView = verticalFOV;
            cam.iso = MainMenuController.PresetData.ISO;
            cam.aperture = MainMenuController.PresetData.Aperture;
            cam.focusDistance = MainMenuController.PresetData.FocusDistance;

            Opening.NumberOfPoints = MainMenuController.PresetData.RaycastAmount;
        }

        NumberOfRoomToGenerate = RoomsGenerationData.NumberOfRoomsToGenerate;
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
            // Update state every second to avoid performance issues
            if (_timeTools2.GetElapsedTimeInSeconds() >= 0.99f)
            {
                InGameMenuController.ElapsedTimeValueLabel.text = _timeTools.GetStringFormattedElapsedTime();
                InGameMenuController.ETAValueLabel.text = FormattedRemainingTime();
                _timeTools2.Start();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }
    }


    /// <summary>
    ///  This method formats the remaining time to generate the rooms. It returns a string with the remaining time in hours, minutes and seconds.
    /// </summary>
    /// <returns></returns>
    private string FormattedRemainingTime()
    {
        float remainingTime = TimeBetween2Screenshots *
                              (DatabaseGenerationData.ScreenshotsNumberPerRoom * NumberOfRoomToGenerate -
                               ScreenshotsIndex);
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
        ScreenshotsIndex = 0;
        _timeTools.Start();
        for (int i = 0; i < RoomsGenerationData.NumberOfRoomsToGenerate; i++)
        {
            RoomIndex = i + 1;
            InGameMenuController.RoomValueLabel.text = RoomIndex + " / " + RoomsGenerationData.NumberOfRoomsToGenerate;
            GameObject go = new GameObject("GeneratedRoom");
            _room = go.AddComponent<ClassicRoom>();
            if (ManualSeeds)
            {
                _room.SetSeeds(_roomSeed, _openingSeed, _objectSeed, _databaseSeed);
            }

            _room.ManualSeeds = ManualSeeds;
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

    #endregion


    #region Information And Objects Getting Methods

    public static float GetOpeningWidth(Vector3 colliderSize)
    {
        return Mathf.Max(colliderSize.x, colliderSize.z);
    }

    #endregion
}