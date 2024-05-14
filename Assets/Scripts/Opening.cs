using System.Collections.Generic;
using UnityEngine;

public class Opening : MonoBehaviour
{
    #region Public Fields

    [HideInInspector] public float OpenessDegree;
    [HideInInspector] public OpeningType Type;
    public MeansOfOpening MeansOfOpening;
    public OpeningDirection OpeningDirection;
    public GameObject MovingPart;


    #endregion

    #region Private Fields

    private float _visibilityRatio;
    private float _width;
    private float _height;
    #endregion
    

    #region Opening Visibility Management Methods

    /// <summary>
    /// Verifies if at least a part of the opening is visible or if the opening is not visible at all on the camera screen.
    /// </summary>
    /// <returns></returns>
    public bool IsVisible()
    {
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer1 in childRenderers)
        {
            if (renderer1.isVisible)
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    public float GetVisibilityRatioBetter()
    {
        _width = RoomsGenerator.GetOpeningWidth(gameObject.GetComponent<BoxCollider>().size);
        _height = gameObject.GetComponent<BoxCollider>().size.y;
        float visibilityRatio = 0f;
        // Get 100 aim points in the opening
        for (float x = -_width / 2f + _width / 20f; x < _width / 2f; x += _width / 10f)
        {
            for (float y = -_height / 2f + _height / 20f; y <= _height / 2f; y += _height / 10f)
            {
                
                Vector3 positionOffset = transform.right * x + transform.up * y;
                Vector3 aimPoint = transform.position + positionOffset;
                if (IsPointVisible(aimPoint) && IsPointOnScreen(aimPoint))
                    visibilityRatio += 0.01f;
                
            }
        }
        return visibilityRatio;
    }

    public BoundingBox2D GetVisibilityBoundingBox()
    {
        _width = RoomsGenerator.GetOpeningWidth(gameObject.GetComponent<BoxCollider>().size);
        _height = gameObject.GetComponent<BoxCollider>().size.y;
        int minX = Screen.width + 1;
        int maxX = -1;
        int minY = Screen.height + 1;
        int maxY = -1;
        
        // Get 100 aim points in the opening
        for (float x = -_width / 2f + _width / 20f; x < _width / 2f; x += _width / 10f)
        {
            for (float y = -_height / 2f + _height / 20f; y <= _height / 2f; y += _height / 10f)
            {
                Vector3 positionOffset = transform.right * x + transform.up * y;
                Vector3 aimPoint = transform.position + positionOffset;
                if (IsPointVisible(aimPoint) && IsPointOnScreen(aimPoint))
                {
                    // Determine the corners of the bounding box
                    Vector3 screenPoint = Camera.main.WorldToScreenPoint(aimPoint);
                    minX = (int)Mathf.Min(minX, screenPoint.x);
                    maxX = (int)Mathf.Max(maxX, screenPoint.x);
                    minY = (int)Mathf.Min(minY, screenPoint.y);
                    maxY = (int)Mathf.Max(maxY, screenPoint.y);
                   
                }
            }
        }
        
        // Return the bounding box (origin is the bottom left corner)
        return new BoundingBox2D(new Vector2Int(minX, minY), maxX - minX, maxY - minY);
    }
    
    private bool IsPointVisible(Vector3 aimPoint)
    {
        GameObject mainCamera = Camera.main.gameObject;
        Vector3 aimPointDirection = aimPoint - mainCamera.transform.position;

        if (Physics.Raycast(mainCamera.transform.position, aimPointDirection, out var hit, float.MaxValue))
        {
            // Check if the ray hits the opening
            if (hit.collider.gameObject == gameObject || hit.collider.gameObject.transform.parent == transform)
                return true;
        }
        return false;
    }
    
    private bool IsPointOnScreen(Vector3 point)
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(point);
        return screenPoint.x is > 0 and < 1 && screenPoint.y is > 0 and < 1 && screenPoint.z > 0;
    }
}

public enum OpeningType
{
    Window,
    Door
}

public enum MeansOfOpening
{
    Translation,
    Rotation
}

public enum OpeningDirection
{
    Up,
    Down,
    Right,
    Left
}
