using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/RoomsGenerationScriptableObject", order = 1)]
public class RoomsGenerationScriptableObject : ScriptableObject  
{
    [Header("Prefabs and objects")]
    public GameObject WallModelPrefab;
    public GameObject HoleBrushPrefab;
    public GameObject BlackShutterPrefab;
    public GameObject OpeningRatioSpherePrefab;
    public List<GameObject> EmptyRooms = new List<GameObject>();
    public List<GameObject> Windows = new List<GameObject>();
    public List<GameObject> Doors = new List<GameObject>();

    [Header("Materials")]
    public List<Material> GroundMaterials = new List<Material>();
    public List<Material> CielingMaterials = new List<Material>();
    public List<Material> WallMaterials = new List<Material>();
    public List<Material> WindowStructureMaterials = new List<Material>();
    public List<Material> DoorFrameMaterials = new List<Material>();
    public List<Material> DoorMaterials = new List<Material>();
    public Dictionary<string, List<Material>> MaterialsDictionary = new Dictionary<string, List<Material>>();

    [Header("Rooms and openings generation information")]
    public int NumberOfEmptyRoomsOnScene;
    public float DistanceBetweenRoomsCenters;
    public float MinimumDistanceBetweenBorders;
    public int WindowPerWallMinimumNumber;
    public int WindowPerWallMaximumNumber;
    public int DoorPerRoomMinimumNumber;
    public int DoorPerRoomMaximumNumber;
    public int DoorPerWallMaximumNumber;
}
