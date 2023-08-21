using System.Collections.Generic;

public class OpeningData
{
    public string Type;
    public Dictionary<string, float> Dimensions = new Dictionary<string, float>();
    public float DistanceToCamera;
    public float OpenessDegree;
    public float VisibilityRatio;
    public BoundingBox2D BoundingBox;
}
