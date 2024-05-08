using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;

public class OpeningData
{
    public string Type;
    public Dictionary<string, float> Dimensions = new Dictionary<string, float>();
    public float DistanceToCamera;
    [JsonConverter(typeof(QuaternionConverter))]
    public UnityEngine.Quaternion RotationQuaternionFromCamera;
    public float OpenessDegree;
    public float VisibilityRatio;
    public BoundingBox2D BoundingBox;
    public BoundingBox2D VisibilityBoundingBox;
    public int MainSeed; // The seed used to generate the room where the opening is placed
    public int DoorSeed; // The seed used to generate the door object
    public int WindowSeed; // The seed used to generate the window object
    public int DBSeed;
}
