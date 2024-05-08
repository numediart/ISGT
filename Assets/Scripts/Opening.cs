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
    public RoomsGenerationScriptableObject RoomsGenerationData;
    public ObjectsGenerationScriptableObject ObjectsGenerationData;


    #endregion

    #region Private Fields

    private List<Renderer> _openingPartsRenderers = new List<Renderer>();
    private GameObject _visibilityRatioSpheresParentObject;
    private float _visibilityRatio;
    private float _width;
    private float _height;
    #endregion

    private void Start()
    {
        // _visibilityRatioSpheresParentObject = GetVisibilityRatioSpheresParentObject();
        _width = RoomsGenerator.GetOpeningWidth(gameObject.GetComponent<BoxCollider>().size);
        _height = gameObject.GetComponent<BoxCollider>().size.y;
        //
        // for (float x = -_width / 2f + _width / 20f; x < _width / 2f; x += _width / 10f)
        // {
        //     for (float y = -_height / 2f + _height / 20f; y <= _height / 2f; y += _height / 10f)
        //     {
        //         Vector3 positionOffset = transform.right * x + transform.up * y;
        //         GameObject ratioSphere = Instantiate(RoomsGenerationData.OpeningRatioSpherePrefab, transform.position + positionOffset, transform.rotation, _visibilityRatioSpheresParentObject.transform);
        //         ratioSphere.transform.localScale = new Vector3(_width / 10f, _height / 10f, _width / 10f + 0.1f);
        //     }
        // }
        //
        // _visibilityRatioSpheresParentObject.SetActive(false);

        // for (int i = 0; i < transform.childCount; i++)
        // {
        //     if (transform.GetChild(i).gameObject.tag.Equals("Untagged") && transform.GetChild(i).gameObject.GetComponent<Renderer>())
        //         _openingPartsRenderers.Add(transform.GetChild(i).gameObject.GetComponent<Renderer>());
        // }
    }

    #region Opening Visibility Management Methods

    /// <summary>
    /// Verifies if at least a part of the openig is visible or if the opening is not visible at all on the camera screen.
    /// </summary>
    /// <returns></returns>
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



    /// <summary>
    /// Initializes the visibility ratio and makes the visibility ratio spheres as visible as possible.
    /// </summary>
    public void InitializeVisibilityRatio()
    {
        _visibilityRatioSpheresParentObject.SetActive(true);

        _visibilityRatio = 0f;

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.tag.Equals("Untagged") && transform.GetChild(i).gameObject.GetComponent<Renderer>())
                transform.GetChild(i).gameObject.GetComponent<Renderer>().enabled = false;
        }

        gameObject.GetComponent<BoxCollider>().enabled = false;
    }

    /// <summary>
    /// Returns the visibility ratio (between 0 and 1) of this opening.
    /// </summary>
    /// <returns></returns>
    public float GetVisibilityRatio()
    {
        Vector3 openingToCamera = Camera.main.transform.position - transform.position;
        float cameraFacingOpeningAngle = Vector3.Angle(-transform.forward, openingToCamera);

        if (Mathf.Abs(cameraFacingOpeningAngle) < 90f)
        {
            for (int i = 0; i < _visibilityRatioSpheresParentObject.transform.childCount; i++)
            {
                if (GetRatioSphereReachability(_visibilityRatioSpheresParentObject.transform.GetChild(i).gameObject) &&
                    DatabaseGenerator.IsOnScreen(_visibilityRatioSpheresParentObject.transform.GetChild(i).gameObject))
                    _visibilityRatio += 0.01f;
            }
        }

        _visibilityRatioSpheresParentObject.SetActive(false);

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.tag.Equals("Untagged") && transform.GetChild(i).gameObject.GetComponent<Renderer>())
                transform.GetChild(i).gameObject.GetComponent<Renderer>().enabled = true;
        }

        gameObject.GetComponent<BoxCollider>().enabled = true;

        return _visibilityRatio;
    }

    /// <summary>
    /// Returns a boolean that indicates if the given visibility ratio sphere is directly visible on the screen or if there are objets 
    /// and/or walls that hide it.
    /// </summary>
    /// <param name="sphere"></param>
    /// <returns></returns>
    private bool GetRatioSphereReachability(GameObject sphere)
    {
        GameObject mainCamera = Camera.main.gameObject;

        Vector3 raycastCenterDirection = sphere.transform.position - mainCamera.transform.position;

        RaycastHit hit;

        if (Physics.Raycast(mainCamera.transform.position, raycastCenterDirection, out hit, float.MaxValue))
        {
            if ((hit.collider.gameObject.tag.Equals("Opening Ratio Sphere") && hit.collider.transform.parent.parent == transform) ||
                (hit.collider.gameObject.transform.parent.parent == transform.parent && hit.collider.gameObject.layer != ObjectsGenerationData.ObjectsLayerIndex))
                return true;
        }

        for (int teta = 0; teta < 360; teta += 90)
        {
            float maxLength = ((teta / 90) % 2 == 0) ? sphere.transform.localScale.y / 2f : sphere.transform.localScale.x / 2f;

            GameObject go = new GameObject();
            go.transform.Rotate(-transform.forward, teta);

            Vector3 pointPosition = sphere.transform.position + go.transform.up.normalized * maxLength;

            Vector3 newDirection = pointPosition - mainCamera.transform.position;

            if (Physics.Raycast(mainCamera.transform.position, newDirection, out hit, float.MaxValue))
            {
                if ((hit.collider.gameObject.tag.Equals("Opening Ratio Sphere") && hit.collider.transform.parent.parent == transform) ||
                    (hit.collider.gameObject.transform.parent.parent == transform.parent && hit.collider.gameObject.layer != ObjectsGenerationData.ObjectsLayerIndex)) 
                {
                    Destroy(go);
                    return true;
                }
            }

            Destroy(go);
        }

        return false;
    }

    private GameObject GetVisibilityRatioSpheresParentObject()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.tag.Equals("Opening Ratio Sphere Parent Object"))
                return child;
        }

        return null;
    }

    #endregion

    public float GetVisibilityRatioBetter()
    {
        float visibilityRatio = 0f;
        // Get 100 aim points in the opening
        for (float x = -_width / 2f + _width / 20f; x < _width / 2f; x += _width / 10f)
        {
            for (float y = -_height / 2f + _height / 20f; y <= _height / 2f; y += _height / 10f)
            {
                Vector3 positionOffset = transform.right * x + transform.up * y;
                Vector3 aimPoint = transform.position + positionOffset;
                if (IsAimPointVisible(aimPoint) && IsPointOnScreen(aimPoint))
                    visibilityRatio += 0.01f;
                
            }
        }
        return visibilityRatio;
    }

    public BoundingBox2D GetVisibilityBoundingBox()
    {
        int minX = Screen.width + 1;
        int maxX = -1;
        int minY = Screen.height + 1;
        int maxY = -1;
        
        for (float x = -_width / 2f + _width / 20f; x < _width / 2f; x += _width / 10f)
        {
            for (float y = -_height / 2f + _height / 20f; y <= _height / 2f; y += _height / 10f)
            {
                Vector3 positionOffset = transform.right * x + transform.up * y;
                Vector3 aimPoint = transform.position + positionOffset;
                if (IsAimPointVisible(aimPoint) && IsPointOnScreen(aimPoint))
                {
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
    
    private bool IsAimPointVisible(Vector3 aimPoint)
    {
        GameObject mainCamera = Camera.main.gameObject;
        Vector3 aimPointDirection = aimPoint - mainCamera.transform.position;

        if (Physics.Raycast(mainCamera.transform.position, aimPointDirection, out var hit, float.MaxValue))
        {
            // Check if the ray hits the opening
            if (hit.collider.gameObject == gameObject || hit.collider.gameObject.transform.parent == transform
                || hit.collider.gameObject.tag.Equals("Opening Ratio Sphere") && hit.collider.transform.parent.parent == transform)
                //TODO : remove check on spheres
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
