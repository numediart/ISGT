using System;
using System.Collections;
using System.Collections.Generic;
using Pro_gen;
using Pro_gen.RoomGrid;
using UnityEngine;
using Utils;
using Random = System.Random;

/// <summary>
/// Abstract class that represents a room in the VISG project.
/// </summary>
public abstract class AbstractRoom<T> : MonoBehaviour
{
    protected RoomsGenerationScriptableObject _roomGenerationData;
    protected GameObject _roomObject;
    protected Vector3 _position;
    protected Quaternion _rotation;
    protected bool _manualSeeds;
    protected Random _roomRandom;
    protected Random _openingRandom;
    protected Random _objectRandom;
    protected Random _databaseRandom;

    protected string _id;
    protected int _roomSeed;
    protected int _openingSeed;
    protected int _objectSeed;
    protected int _databaseSeed;

    protected RoomGrid _roomGrid;
    protected ProceduralPropPlacer _proceduralPropPlacer;

    protected SeedsProvider _seedsProvider;

    protected int _roomIdx;

    public DatabaseGenerationScriptableObject DatabaseGenerationData;
    public bool ManualSeeds
    {
        set => _manualSeeds = value;
    }

    public int RoomSeed => _roomSeed;
    public int OpeningSeed => _openingSeed;
    public int ObjectSeed => _objectSeed;
    public int DatabaseSeed => _databaseSeed;

    public int DeltaTimeRoomGeneration { get; protected set; }

    public RoomState RoomState { get; set; }
    public Random DatabaseRandom => _databaseRandom;
    public string Id => _id;
    public GameObject RoomObject => _roomObject;

    public List<Bounds> EmptyQuadNodesCenters { get; set; }

    public RoomGrid RoomGrid => _roomGrid;

    protected virtual void Awake()
    {
        _objectRandom = new Random(_objectSeed);
        _databaseRandom = new Random(_databaseSeed);
        _roomRandom = new Random(_roomSeed);
        _openingRandom = new Random(_openingSeed);
        _roomObject = gameObject;
        _roomObject.AddComponent<RoomGrid>();
        _roomObject.AddComponent<ProceduralPropPlacer>();
        RoomState = RoomState.Empty;
    }

    /// <summary>
    /// init method of the Room class.
    /// </summary>
    /// <param name="roomGenerationData"></param>
    public abstract void InitRoom(RoomsGenerationScriptableObject roomGenerationData,
        DatabaseGenerationScriptableObject databaseGenerationData);

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
    protected abstract void CreateOpenings();

    /// <summary>
    /// This method fills the room with objects.
    /// </summary>
    public abstract void FillRoomWithObjects();

    protected abstract IEnumerator GenerateDatabase();

    /// <summary>
    /// Init seeds for the room, openings, objects and database. Method is called in the constructor.
    /// </summary>
    protected abstract void InitSeeds();

    public abstract T GetRoom { get; }
}
