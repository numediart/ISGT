using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class Opening : MonoBehaviour
{
    #region Public Fields

    [HideInInspector] public float OpennessDegree;
    public OpeningType Type;
    public MeansOfOpening MeansOfOpening;
    public GameObject MovingPart;

    #endregion

    #region Private Fields

    private float _visibilityRatio;
    private float _width;
    private float _height;
    
    //TODO : make this a parameter
    private static readonly float _numberOfPoints = 1000f;

    #endregion

    public void Start()
    {
        SetOpeningRandomOpenness();
    }

    private void SetOpeningRandomOpenness()
    {
        OpennessDegree = UnityEngine.Random.value;

        Vector3 openingDirection = transform.up;

        switch (MeansOfOpening)
        {
            case MeansOfOpening.Translation:
                Vector3 openingDimensions = MovingPart.transform.GetChild(0).localScale;
                float sideLength = openingDimensions.y;
                MovingPart.transform.position += OpennessDegree * (sideLength / 2f) * openingDirection.normalized;
                MovingPart.transform.GetChild(0).localScale = new Vector3(openingDimensions.x, (1 - OpennessDegree) * sideLength, openingDimensions.z);
                break;
            case MeansOfOpening.Rotation:
                float adjustedOpenness = (OpennessDegree * 2f) - 1f;
                MovingPart.transform.Rotate(openingDirection, adjustedOpenness * 120f);
                break;
        }
    }

    #region Opening Visibility Management Methods

    public bool IsVisible()
    {
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in childRenderers)
        {
            if (renderer.isVisible)
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    public float GetVisibilityRatio()
    {
        return _visibilityRatio;
    }
    
   
  
    public BoundingBox2D GetVisibilityBoundingBox()
    {

       BoxCollider openingBounds = gameObject.GetComponent<BoxCollider>();
        _width = RoomsGenerator.GetOpeningWidth(openingBounds.size);
        _height = openingBounds.size.y;
        int minX = Screen.width + 1;
        int maxX = -1;
        int minY = Screen.height + 1;
        int maxY = -1;
        _visibilityRatio = 0f;

        float widthStep = _width / Mathf.Sqrt(_numberOfPoints);
        float heightStep = _height / Mathf.Sqrt(_numberOfPoints);

        for (float x = -_width / 2f + widthStep / 2; x < _width / 2f; x += widthStep)
        {
            for (float y = -_height / 2f + heightStep / 2; y <= _height / 2f; y += heightStep)
            {
                Vector3 positionOffset = transform.right * x + transform.up * y;
                Vector3 aimPoint = transform.position + positionOffset;
                if (IsPointVisible(aimPoint) && IsPointOnScreen(aimPoint))
                {
                    _visibilityRatio += 1 / _numberOfPoints;

                    Vector3 screenPoint = Camera.main.WorldToScreenPoint(aimPoint);
                    minX = (int)Mathf.Min(minX, screenPoint.x);
                    maxX = (int)Mathf.Max(maxX, screenPoint.x);
                    minY = (int)Mathf.Min(minY, screenPoint.y);
                    maxY = (int)Mathf.Max(maxY, screenPoint.y);
                }
            }
        }

        return new BoundingBox2D(new Vector2Int(minX, minY), maxX - minX, maxY - minY);
    }

    private bool IsPointVisible(Vector3 aimPoint)
    {
        GameObject mainCamera = Camera.main.gameObject;
        Vector3 aimPointDirection = aimPoint - mainCamera.transform.position;

        if (Physics.Raycast(mainCamera.transform.position, aimPointDirection, out var hit, float.MaxValue))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.gameObject.transform.parent == transform)
                return true;
        }
        return false;
    }

    private bool IsPointOnScreen(Vector3 point)
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(point);
        return screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1 && screenPoint.z > 0;
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
