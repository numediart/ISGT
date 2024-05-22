using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/ProGenParams", order = 3)]
public class ProGenParams:ScriptableObject
{
    [Header("Room Generation Parameters")]
    [Tooltip("Width of the grid"), Min(3)]
    public int width;
    [Tooltip("Height of the grid") , Min(3)]
    public int height;
    
    [Tooltip("Width offset is the length of one side of the wall prefab e.g 2.5f")]
    public float widthOffset=2.5f;
    [Tooltip("Height offset is the length of one side of the wall prefab e.g 2.5f")]
    public float heightOffset=2.5f;
    
    [Header("Room Generation Seeds")]
    public int roomSeed;
    
    [Header("Prefabs")]
    public RoomCell RoomCellPrefab;

    public List<GameObject> WallDoorPrefabs;
    public List<GameObject> WallWindowsPrefabs;

    [Header("Custom Room Generation Parameters")]
    public int WindowNumberRatio;
    public int DoorNumberRatio;
    public int ObjectNumberRatio;
    public int WindowPerWallNumber;
    public int DoorPerWallNumber;
}