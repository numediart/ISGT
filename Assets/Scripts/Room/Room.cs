using System;
using System.Collections.Generic;
using Pro_gen;
using UnityEngine;
using Utils;
using Random = System.Random;

/// <summary>
/// Class that represents a room in the VISG project.
/// </summary>
public class Room : MonoBehaviour
{
    private ProGenParams _proGenParams;
    private RoomsGenerationScriptableObject roomGenerationData;
    private GameObject _roomObject; //
    private Vector3 _position;
    private Quaternion _rotation;

    private bool _manualSeeds;
    private Random _roomRandom;
    private Random _openingRandom;
    private Random _objectRandom;
    private Random _databaseRandom;

    [SerializeField] private string _id; //unique id of the room (use for screenshot data)
    [SerializeField] private int _roomSeed;
    [SerializeField] private int _openingSeed;
    [SerializeField] private int _objectSeed;
    [SerializeField] private int _databaseSeed;

     private RoomGrid _roomGrid;
     private ProceduralPropPlacer _proceduralPropPlacer;
    
    private SeedsProvider _seedsProvider;

    private int _roomIdx;

    public bool ManualSeeds
    {
        set => _manualSeeds = value;
    }

    public int RoomSeed => _roomSeed;
    public int OpeningSeed => _openingSeed;
    public int ObjectSeed => _objectSeed;
    public int DatabaseSeed => _databaseSeed;

    public Random DatabaseRandom => _databaseRandom;
    public string Id => _id; //unique id of the room (use for screenshot data)
    public GameObject RoomObject => _roomObject;


    private void Awake()
    {
        _objectRandom = new Random(_objectSeed);
        _databaseRandom = new Random(_databaseSeed);
        _roomRandom = new Random(_roomSeed);
        _openingRandom = new Random(_openingSeed);
        _roomObject = gameObject;
        _roomObject.AddComponent<RoomGrid>();
        _roomObject.AddComponent<ProceduralPropPlacer>();
    }
    
    /// <summary>
    /// init method of the Room class.
    /// </summary>
    /// <param name="roomGenerationData"></param>

    public void InitRoom(RoomsGenerationScriptableObject roomGenerationData)
    {
        this.roomGenerationData = roomGenerationData;
        _seedsProvider = new SeedsProvider();
        _id = Guid.NewGuid().ToString();
        if (!_manualSeeds)
        {
            InitSeeds();
        }
        
        
        name = "Room_" + _id;
        bool isRoomGridComponent =TryGetComponent<RoomGrid>(out _roomGrid);
        if(!isRoomGridComponent)
        {
            Debug.Log("Room Grid is null");
            return;
        }
        _roomGrid.InitGrid( _roomSeed,this.roomGenerationData);
        TryGetComponent<ProceduralPropPlacer>(out _proceduralPropPlacer);
        _proceduralPropPlacer.Init(this.roomGenerationData);
        CreateOpenings();
        FillRoomWithObjects();
    }

    public void SetSeeds(int roomSeed, int openingSeed, int objectSeed, int databaseSeed)
    {
        _manualSeeds = true;
        _roomSeed = roomSeed;
        _openingSeed = openingSeed;
        _objectSeed = objectSeed;
        _databaseSeed = databaseSeed;
        _roomRandom = new Random(_roomSeed);
        _openingRandom = new Random(_openingSeed);
        _objectRandom = new Random(_objectSeed);
        _databaseRandom = new Random(_databaseSeed);
    }

    /// <summary>
    /// This method creates the openings in the room.
    /// </summary>
    private void CreateOpenings()
    {
        TimeTools timeTools = new TimeTools();
        timeTools.Start();
        _roomGrid.RandomReplaceActiveWallWithDoorConstraint(
            _openingRandom); // replace the active wall with door with the constraint of the distance between the doors
        _roomGrid.RandomReplaceActiveWithWindowConstraint(
            _openingRandom); // replace the active wall with window with the constraint of the distance between the windows
        _roomGrid.ApplyTextures();
        timeTools.Stop();
        Debug.Log("Time to create openings: " + timeTools.GetElapsedTime());
    }

    /// <summary>
    /// This method fills the room with objects.
    /// </summary>
    public void FillRoomWithObjects()
    {
        TimeTools timeTools = new TimeTools();
        timeTools.Start();
        _proceduralPropPlacer.PlaceProps(_objectRandom, roomGenerationData.width * roomGenerationData.height); // place the props in the room
        timeTools.Stop();
        Debug.Log("Time to place objects: " + timeTools.GetElapsedTime());
    }

    /// <summary>
    /// Init seeds for the room, openings, objects and database. Method is called in the constructor.
    /// </summary>
    private void InitSeeds()
    {
        _roomSeed = _seedsProvider.GetSeed();
        _roomRandom = new Random(_roomSeed);
        _openingSeed = _seedsProvider.CreateSubSeed();
        _openingRandom = new Random(_openingSeed);
        _objectSeed = _seedsProvider.CreateSubSeed();
        _objectRandom = new Random(_objectSeed);
        _databaseSeed = _seedsProvider.CreateSubSeed();
        _databaseRandom = new Random(_databaseSeed);
    }

    public Room GetRoom => this; // return the Room instance
    
}