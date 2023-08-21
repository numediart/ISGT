using System.Collections.Generic;

public class ObjectToPlace
{
    public string Id;
    public float[] PositionRelativeToRoomOrigin = new float[3];
    public Dictionary<string, float> Dimensions = new Dictionary<string, float>();
    public float[] Angles = new float[3];

    public ObjectToPlace(string id, float[] positionRelativeToRoomOrigin, Dictionary<string,float> dimensions, float[] angles)
    {
        Id = id;
        PositionRelativeToRoomOrigin = positionRelativeToRoomOrigin;
        Dimensions = dimensions;
        Angles = angles;
    }
}
