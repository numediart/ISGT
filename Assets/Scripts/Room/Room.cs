using System;
using UnityEngine;
using Utils;
using Random = System.Random;

    /// <summary>
    /// Class that represents a room in the VISG project.
    /// </summary>
    public class Room : ScriptableObject
    {
        OpeningsGenerator _openingsGenerator;
        ObjectsGenerator _objectsGenerator;
        private RoomsGenerationScriptableObject roomGenerationData;
        private string _id;//unique id of the room (use for screenshot data)
        private GameObject _roomObject; //
        private Vector3 _position;
        private Quaternion _rotation;

        private Random _roomRandom;
        private Random _openingRandom;
        private Random _objectRandom;
        private Random _databaseRandom;
        private int _roomSeed;
        private int _openingSeed;
        private int _objectSeed;
        private int _databaseSeed;
        private SeedsProvider _seedsProvider;
        private int _roomIdx;
        
        public int RoomSeed => _roomSeed;
        public int OpeningSeed => _openingSeed;
        public int ObjectSeed => _objectSeed;
        public int DatabaseSeed => _databaseSeed;
        public string Id => _id;//unique id of the room (use for screenshot data)
        public GameObject RoomObject => _roomObject;

        private void Awake()
        {
            GameObject generatorsContainer = GameObject.Find("GeneratorsContainer");
            roomGenerationData=  generatorsContainer.GetComponent<GeneratorsContainer>().RoomsGenerator.RoomsGenerationData;
            _openingsGenerator= generatorsContainer.GetComponent<GeneratorsContainer>().OpeningsGenerator;
            _objectsGenerator= generatorsContainer.GetComponent<GeneratorsContainer>().ObjectsGenerator;
            _seedsProvider = new SeedsProvider();
            _id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Constructor of the Room class.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="roomGenerationData"></param>
        public void InitRoom(int roomIdx)
        {
            _roomIdx = roomIdx;
            InitSeeds();
            InitEmptyRoom();
            CreateOpenings();
            FillRoomWithObjects();
        }

        
          
        

        /// <summary>
        /// This method initializes an empty room.
        /// </summary>
        /// <param name="roomGenerationData"></param>
        private void InitEmptyRoom()
        {
            
            _position = new Vector3(_roomIdx * roomGenerationData.DistanceBetweenRoomsCenters, 0, 0);// set position of the room based on its id
            _rotation = Quaternion.identity;// set rotation of the room (no rotation)
            _roomObject = Instantiate(roomGenerationData.EmptyRooms[_roomRandom.Next(0,roomGenerationData.EmptyRooms.Count)], _position, _rotation);// instantiate the room object
            Debug.Log("Room created" + _roomObject.gameObject.name);
            _roomObject.name = "Room" + _roomIdx;// set the name of the room object
        }
        
        /// <summary>
        /// This method creates the openings in the room.
        /// </summary>
        private void CreateOpenings()
        { 
            Debug.Log("Creating openings");
            _openingsGenerator.OpeningsGeneration(_roomObject, _openingRandom);
            Debug.Log("Openings created");
        }
        /// <summary>
        /// This method fills the room with objects.
        /// </summary>
        private void FillRoomWithObjects()
        {
            Debug.Log("Filling room with objects");
            _objectsGenerator.ObjectsGeneration(_roomObject, _objectRandom);
            Debug.Log("Room filled with objects");
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
        
        public Room GetRoom => this;// return the Room instance
    }
