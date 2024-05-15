using System;
using UnityEngine;
using Utils;
using Random = System.Random;

    /// <summary>
    /// Class that represents a room in the VISG project.
    /// </summary>
    public class Room : MonoBehaviour
    {
        OpeningsGenerator _openingsGenerator;
        ObjectsGenerator _objectsGenerator;
        private RoomsGenerationScriptableObject roomGenerationData;
        [SerializeField] private string _id;//unique id of the room (use for screenshot data)
        private GameObject _roomObject; //
        private Vector3 _position;
        private Quaternion _rotation;

        private bool _manualSeeds;
        private Random _roomRandom;
        private Random _openingRandom;
        private Random _objectRandom;
        private Random _databaseRandom;

        [SerializeField] private int _roomSeed;
        [SerializeField] private int _openingSeed;
        [SerializeField] private int _objectSeed;
        [SerializeField] private int _databaseSeed;
        
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
        public string Id => _id;//unique id of the room (use for screenshot data)
        public GameObject RoomObject => _roomObject;
        public OpeningsGenerator OpeningsGenerator => _openingsGenerator;
        public ObjectsGenerator ObjectsGenerator => _objectsGenerator;


        private void Awake()
        {
            GameObject generatorsContainer = GameObject.Find("GeneratorsContainer");
            _objectsGenerator= generatorsContainer.GetComponent<GeneratorsContainer>().ObjectsGenerator;
            _openingsGenerator= generatorsContainer.GetComponent<GeneratorsContainer>().OpeningsGenerator;
            roomGenerationData=  generatorsContainer.GetComponent<GeneratorsContainer>().RoomsGenerator.RoomsGenerationData;
            _objectRandom = new Random(_objectSeed);
            _databaseRandom = new Random(_databaseSeed);
            _roomRandom = new Random(_roomSeed);
            _openingRandom = new Random(_openingSeed);
            _roomObject = gameObject;
        }

        public Room Copy(Room other)
        {
            other._roomSeed = _roomSeed;
            other._openingSeed = _openingSeed;
            other._objectSeed = _objectSeed;
            other._databaseSeed = _databaseSeed;
            other._roomRandom = new Random(_roomSeed);
            other._openingRandom = new Random(_openingSeed);
            other._objectRandom = new Random(_objectSeed);
            other._databaseRandom = new Random(_databaseSeed);
            
            other._id = _id;
            other._roomObject = _roomObject;
            other._position = _position;
            
            other._rotation = _rotation;
            other._roomIdx = _roomIdx;
            other._seedsProvider = _seedsProvider;
            other._openingsGenerator = _openingsGenerator;
            other._objectsGenerator = _objectsGenerator;
            other._databaseSeed = _databaseSeed;
            other.roomGenerationData = roomGenerationData;
            return other;
        } 
        /// <summary>
        /// init method of the Room class.
        /// </summary>
        /// <param name="roomIdx"></param>

        public void InitRoom(int roomIdx)
        {
            GameObject generatorsContainer = GameObject.Find("GeneratorsContainer");
            roomGenerationData=  generatorsContainer.GetComponent<GeneratorsContainer>().RoomsGenerator.RoomsGenerationData;
            _openingsGenerator= generatorsContainer.GetComponent<GeneratorsContainer>().OpeningsGenerator;
            _seedsProvider = new SeedsProvider();
            _id = Guid.NewGuid().ToString();
            _roomIdx = roomIdx;
            if (!_manualSeeds) { InitSeeds(); }
            InitEmptyRoom();
            CreateOpenings();
            // FillRoomWithObjects();// fill the room with objects (runtime)
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
        /// This method initializes an empty room.
        /// </summary>
        private void InitEmptyRoom()
        {
            
            _position = new Vector3(_roomIdx * roomGenerationData.DistanceBetweenRoomsCenters, 0, 0);// set position of the room based on its id
            _rotation = Quaternion.identity;// set rotation of the room (no rotation)
            _roomObject = Instantiate(roomGenerationData.EmptyRooms[_roomRandom.Next(0,roomGenerationData.EmptyRooms.Count)], _position, _rotation);// instantiate the room object
            _roomObject.name = "Room_" + _roomIdx;// set the name of the room object
        }
        
        /// <summary>
        /// This method creates the openings in the room.
        /// </summary>
        private void CreateOpenings()
        { 
            _openingsGenerator.OpeningsGeneration(_roomObject, _openingRandom);
        }
        /// <summary>
        /// This method fills the room with objects.
        /// </summary>
        public void FillRoomWithObjects()
        {
            _objectsGenerator.ObjectsGeneration(_roomObject, _objectRandom);
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
