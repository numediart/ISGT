using System.Collections.Generic;
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
}
