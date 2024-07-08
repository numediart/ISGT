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
    
    public static float NumberOfPoints;

    #endregion

    private void Awake()
    {
        _mainCamera = Camera.main;
        if (centerObj == null)
        {
            Debug.LogError("Center object not assigned for opening : " + gameObject.name);
        }
        SetOpeningRandomOpenness();
        Debug.Log(NumberOfPoints);
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

        float widthStep = _width / Mathf.Sqrt(NumberOfPoints);
        float heightStep = _height / Mathf.Sqrt(NumberOfPoints);

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

    
    /// <summary>
    /// Get the  bounding box in pixels of a given opening on a screenshot, based only on visible parts.
    /// </summary>
    /// <returns> The bounding box 2D in pixels of the visible part of the opening on a screenshot. </returns>
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

        float widthStep = _width / Mathf.Sqrt(NumberOfPoints);
        float heightStep = _height / Mathf.Sqrt(NumberOfPoints);

        for (float x = -_width / 2f + widthStep / 2; x < _width / 2f; x += widthStep)
        {
            for (float y = -_height / 2f + heightStep / 2; y <= _height / 2f; y += heightStep)
            {
                var thisTransform = transform;
                Vector3 positionOffset = thisTransform.right * x + thisTransform.up * y;
                Vector3 aimPoint = GetCenter() + positionOffset;
                if (IsPointVisible(aimPoint) && IsPointOnScreen(aimPoint))
                {
                    _visibilityRatio += 1 / NumberOfPoints;

                    Vector3 screenPoint = _mainCamera.WorldToScreenPoint(aimPoint);
                    minX = (int)Mathf.Min(minX, screenPoint.x);
                    maxX = (int)Mathf.Max(maxX, screenPoint.x);
                    minY = (int)Mathf.Min(minY, screenPoint.y);
                    maxY = (int)Mathf.Max(maxY, screenPoint.y);
                }
            }
        }
        // 640 * 360 is the minimum resolution
        int screenShotWidth = 640 * MainMenuController.PresetData.Resolution;
        int screenShotHeight = 360 * MainMenuController.PresetData.Resolution;
        
        // Scale coordinates to screenshot size
        minX = (int)(minX * screenShotWidth / Screen.width);
        maxX = (int)(maxX * screenShotWidth / Screen.width);
        minY = (int)(minY * screenShotHeight / Screen.height);
        maxY = (int)(maxY * screenShotHeight / Screen.height);


        return new BoundingBox2D(new Vector2Int(minX, minY), maxX - minX, maxY - minY);
    }
    
    /// <summary>
    /// Get the full bounding box in pixels of a given opening on a screenshot.
    /// </summary>
    /// <returns> The bounding box 2D in pixels of the opening on a screenshot. </returns>
    public BoundingBox2D GetFullBoundingBox()
    {
        Camera _camera = Camera.main;
        
        if (!TryGetComponent<Opening>(out Opening openingComponent) ||
            !TryGetComponent<BoxCollider>(out BoxCollider boxCollider))
        {
            return null;
        }

        Vector3 openingPosition = openingComponent.GetCenter();
        Vector3 colliderSize = boxCollider.size;
        float width = RoomsGenerator.GetOpeningWidth(colliderSize);
        float height = colliderSize.y;

        Vector3[] corners = new Vector3[4];
        var right = transform.right;
        var up = transform.up;
        corners[0] = openingPosition - right * width / 2f - up * height / 2f;
        corners[1] = openingPosition + right * width / 2f - up * height / 2f;
        corners[2] = openingPosition + right * width / 2f + up * height / 2f;
        corners[3] = openingPosition - right * width / 2f + up * height / 2f;

        Vector3[] screenCorners = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            screenCorners[i] = _camera.WorldToScreenPoint(corners[i]);
            if (screenCorners[i].z < 0)
            {
                Vector3 distVector =
                    Vector3.Project(
                        _camera.transform.position + _camera.transform.forward * _camera.nearClipPlane - corners[i],
                        _camera.transform.forward);
                screenCorners[i] = _camera.WorldToScreenPoint(corners[i] + distVector);
            }
        }

        float minX = Mathf.Min(screenCorners[0].x, screenCorners[1].x, screenCorners[2].x, screenCorners[3].x);
        float minY = Mathf.Min(screenCorners[0].y, screenCorners[1].y, screenCorners[2].y, screenCorners[3].y);
        float maxX = Mathf.Max(screenCorners[0].x, screenCorners[1].x, screenCorners[2].x, screenCorners[3].x);
        float maxY = Mathf.Max(screenCorners[0].y, screenCorners[1].y, screenCorners[2].y, screenCorners[3].y);

        int screenWidth = _camera.pixelWidth;
        int screenHeight = _camera.pixelHeight;

        if (minX > screenWidth || minY > screenHeight || maxX < 0 || maxY < 0)
        {
            return null;
        }

        Vector2Int boundingBoxOrigin =
            new Vector2Int(Mathf.Clamp((int)minX, 0, screenWidth), Mathf.Clamp((int)minY, 0, screenHeight));
        int boxWidth = Mathf.Clamp((int)maxX, 0, screenWidth) - boundingBoxOrigin.x;
        int boxHeight = Mathf.Clamp((int)maxY, 0, screenHeight) - boundingBoxOrigin.y;
        
        // 640 * 360 is the minimum resolution
        int screenShotWidth = 640 * MainMenuController.PresetData.Resolution;
        int screenShotHeight = 360 * MainMenuController.PresetData.Resolution;
        
        // Scale coordinates to screenshot size
        boundingBoxOrigin = new Vector2Int((boundingBoxOrigin.x * screenShotWidth / screenWidth),
            (boundingBoxOrigin.y * screenShotHeight / screenHeight));
        boxWidth = (boxWidth * screenShotWidth / screenWidth);
        boxHeight = (boxHeight * screenShotHeight / screenHeight);

        return new BoundingBox2D(boundingBoxOrigin, boxWidth, boxHeight);
    }
    
    // Check if a point is visible from the camera
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
    
    // Check if a point is on the screen, i.e. in the camera's view frustum
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