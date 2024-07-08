using System;
using System.Collections;
using Pro_gen;
using Pro_gen.RoomGrid;
using UnityEngine;
using Utils;
using Random = System.Random;

/// <summary>
/// Class that represents a classic room in the ISGT project.
/// </summary>
public class ClassicRoom : AbstractRoom<ClassicRoom>
{
    private new void Awake()
    {
        _objectRandom = new Random(_objectSeed);
        _databaseRandom = new Random(_databaseSeed);
        _roomRandom = new Random(_roomSeed);
        _openingRandom = new Random(_openingSeed);
        _roomObject = gameObject;
        _roomObject.AddComponent<RoomGrid>();
        _roomObject.AddComponent<ProceduralPropPlacer>();
        _roomObject.AddComponent<DatabaseGenerator>();
        RoomState = RoomState.Empty;
    }

    /// <summary>
    /// init method of the Room class.
    /// </summary>
    /// <param name="roomGenerationData"></param>
    /// <param name="databaseGenerationData"></param>
    public override void InitRoom(RoomsGenerationScriptableObject roomGenerationData,
        DatabaseGenerationScriptableObject databaseGenerationData)
    {
        this._roomGenerationData = roomGenerationData;
        this.DatabaseGenerationData = databaseGenerationData;
        _seedsProvider = new SeedsProvider();
        _id = Guid.NewGuid().ToString();
        if (!_manualSeeds)
        {
            InitSeeds();
        }

        name = "Room_" + _id;
        bool isRoomGridComponent = TryGetComponent(out _roomGrid);
        if (!isRoomGridComponent)
        {
            Debug.Log("Room Grid is null");
            return;
        }

        TimeTools timeTools = new TimeTools();
        timeTools.Start();
        _roomGrid.InitGrid(_roomSeed, this._roomGenerationData);
        TryGetComponent(out _proceduralPropPlacer);
        _proceduralPropPlacer.Init(this._roomGenerationData);
        CreateOpenings();
        FillRoomWithObjects();
        timeTools.Stop();
        DeltaTimeRoomGeneration = timeTools.GetElapsedTime();
        StartCoroutine(GenerateDatabase());
    }
    
    /// <summary>
    /// This method creates the openings in the room.
    /// </summary>
    protected override void CreateOpenings()
    {
        TimeTools timeTools = new TimeTools();
        timeTools.Start();
        _roomGrid.RandomReplaceActiveWallWithDoorConstraint(
            _openingRandom); // replace the active wall with door with the constraint of the distance between the doors
        _roomGrid.RandomReplaceActiveWithWindowConstraint(
            _openingRandom); // replace the active wall with window with the constraint of the distance between the windows
        _roomGrid.ApplyTextures();
        timeTools.Stop();
        Debug.Log("Time to create openings: " + timeTools.GetElapsedTimeInSeconds() + " seconds");
    }

    /// <summary>
    /// This method fills the room with objects.
    /// </summary>
    public override void FillRoomWithObjects()
    {
        TimeTools timeTools = new TimeTools();
        timeTools.Start();
        StartCoroutine(_proceduralPropPlacer.PlaceProps(_objectRandom,
            _roomGenerationData.width * _roomGenerationData.height)); // place the props in the room
        timeTools.Stop();
        Debug.Log("Time to place objects: " + timeTools.GetElapsedTime());
    }

    
    /// <summary>
    /// Generate the database of the room.(take screenshots, generate the database and save it in a json file)
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator GenerateDatabase()
    {
        TryGetComponent<DatabaseGenerator>(out var databaseGenerator);
        yield return new WaitUntil(() => RoomState == RoomState.Filled);
        EmptyQuadNodesCenters = _proceduralPropPlacer.GetAllEmptyQuadNodes();
        databaseGenerator.Init(GetComponent<ClassicRoom>());
        StartCoroutine(databaseGenerator.DatabaseGeneration());
    }

    /// <summary>
    /// Init seeds for the room, openings, objects and database. Method is called in the constructor.
    /// </summary>
    protected override void InitSeeds()
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

    public override ClassicRoom GetRoom => this; // return the Room instance
}

// create enum for Room State
public enum RoomState
{
    Empty,
    Filled,
    DatabaseGenerated
}