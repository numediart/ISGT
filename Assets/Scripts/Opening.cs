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
    public GameObject Structure;
    public GameObject centerObj;

    #endregion

    #region Private Fields

    private float _visibilityRatio;
    private float _width;
    private float _height;
    private Camera _mainCamera;

    //TODO : make this a parameter
    private static readonly float _numberOfPoints = 1000f;

    #endregion

    private void Awake()
    {
        _mainCamera = Camera.main;
        if (centerObj == null)
        {
            Debug.LogError("Center object not assigned for opening : " + gameObject.name);
        }
        SetOpeningRandomOpenness();
    }
    
    public Vector3 GetCenter()
    {
        return centerObj.transform.position;
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
                MovingPart.transform.GetChild(0).localScale = new Vector3(openingDimensions.x,
                    (1 - OpennessDegree) * sideLength, openingDimensions.z);
                break;
            case MeansOfOpening.Rotation:
                float adjustedOpenness = (OpennessDegree * 2f) - 1f;
                MovingPart.transform.Rotate(openingDirection, adjustedOpenness * 120f);
                break;
        }
    }

    #region Opening Visibility Management Methods

    public bool IsOnScreen()
    {
        gameObject.TryGetComponent<BoxCollider>(out BoxCollider openingBounds);
        _width = RoomsGenerator.GetOpeningWidth(openingBounds.size);
        _height = openingBounds.size.y;

        float widthStep = _width / Mathf.Sqrt(_numberOfPoints);
        float heightStep = _height / Mathf.Sqrt(_numberOfPoints);

        for (float x = -_width / 2f + widthStep / 2; x < _width / 2f; x += widthStep)
        {
            for (float y = -_height / 2f + heightStep / 2; y <= _height / 2f; y += heightStep)
            {
                var thisTransform = transform;
                Vector3 positionOffset = thisTransform.right * x + thisTransform.up * y;
                Vector3 aimPoint = GetCenter() + positionOffset;
                if (IsPointOnScreen(aimPoint))
                {
                    return true;
                }
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
        gameObject.TryGetComponent<BoxCollider>(out BoxCollider openingBounds);
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
                var thisTransform = transform;
                Vector3 positionOffset = thisTransform.right * x + thisTransform.up * y;
                Vector3 aimPoint = GetCenter() + positionOffset;
                if (IsPointVisible(aimPoint) && IsPointOnScreen(aimPoint))
                {
                    _visibilityRatio += 1 / _numberOfPoints;

                    Vector3 screenPoint = _mainCamera.WorldToScreenPoint(aimPoint);
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
        GameObject mainCamera = _mainCamera!.gameObject;
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
        Vector3 screenPoint = _mainCamera!.WorldToViewportPoint(point);
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