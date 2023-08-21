using UnityEngine;

public class BoundingBox2D
{
    public int[] Origin;
    public int[] Dimension;

    public BoundingBox2D(int[] origin, int[] dimension)
    {
        Origin = origin;
        Dimension = dimension;
    }

    public BoundingBox2D(Vector2Int origin, int boxWidth, int boxHeight)
    {
        Origin = new int[] { origin.x, origin.y };
        Dimension = new int[] { boxWidth, boxHeight };
    }
}
