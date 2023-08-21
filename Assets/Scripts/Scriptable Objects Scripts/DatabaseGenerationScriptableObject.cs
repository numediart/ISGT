using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/DatabaseGenerationScriptableObject", order = 3)]
public class DatabaseGenerationScriptableObject : ScriptableObject  
{
    [Header("Screenshots making information")]
    public int ScreenshotsNumberPerRoom;
    [HideInInspector] public float TimeBeforeScreenshotsTakingBeginning = 1f;
    [HideInInspector] public float TimeBetweenCameraPlacementAndScreenshot = 0.01f;
    [HideInInspector] public float TimeBetweenScreenshotAndDataGetting = 0.01f;
    [HideInInspector] public float TimeBetweenInitializationAndDataGetting = 0.02f;
    [HideInInspector] public float TimeBetweenVisibilityRatioAndBoundingBox = 0.02f;
    [HideInInspector] public float TimeMargin = 0.2f;

    [Header("Camera placement requirements")]
    public float CameraMinimumDistanceFromWall;
    public float CameraMinimumDistanceFromGroundAndCieling;
    public float CameraMinimumDistanceFromObjects;
    public float MaximumCameraXRotation;
    public float MaximumCameraYRotation;
    public float MaximumCameraZRotation;
}
