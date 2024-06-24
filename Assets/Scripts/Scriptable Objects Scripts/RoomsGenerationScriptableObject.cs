using System.Collections.Generic;
using Pro_gen;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/RoomsGenerationScriptableObject", order = 1)]
public class RoomsGenerationScriptableObject : ScriptableObject  
{
    [Header("Room generation parameters")]
    [Tooltip("Max width of a generated room"), Range(4,20)] 
    public int MaxRoomWidth;
    [Tooltip("Max height of a generated room"), Range(4,20)]
    public int MaxRoomHeight;
    
    [HideInInspector] [SerializeField] public int width;
    [HideInInspector] [SerializeField] public int height;
    
    
    [Tooltip("Width offset is the length of one side of the wall prefab e.g 2.5f")]
    public float widthOffset=2.5f;
    [Tooltip("Height offset is the length of one side of the wall prefab e.g 2.5f")]
    public float heightOffset=2.5f;
    
    public int _gridSubdivision = 1;
    [Header("Prefabs")]
    public RoomCell RoomCellPrefab;

    public List<GameObject> WallDoorPrefabs;
    public List<GameObject> WallWindowsPrefabs;
    
    public List<Props> PropsPrefabs;
    
    [Header("Textures")]
    public List<Material> WallMaterials;
    public List<Material> FloorMaterials;
    public List<Material> CeilingMaterials;
    public List<Material> WindowMaterials;



    [Header("Rooms and openings generation information")]
    public int ObjectNumberRatio;
    public int WindowPerWallNumber;
    public int DoorPerWallNumber;
    
    public int NumberOfEmptyRoomsOnScene; 
    public float MinimumDistanceBetweenBorders;
    public int WindowPerWallMinimumNumber;
    public int WindowPerWallMaximumNumber;
    public int DoorPerRoomMinimumNumber;
    public int DoorPerRoomMaximumNumber;
    public int DoorPerWallMaximumNumber;
    public int WindowPerRoomMinimumNumber;
    public int WindowPerRoomMaximumNumber;
}
