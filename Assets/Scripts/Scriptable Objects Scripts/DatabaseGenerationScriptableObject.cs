using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/DatabaseGenerationScriptableObject", order = 3)]
public class DatabaseGenerationScriptableObject : ScriptableObject  
{
    [Header("Screenshots making information")]
    public int ScreenshotsNumberPerRoom;
    [HideInInspector] public float TimeBetweenScreenshotsInManualMode = 0.25f;

    [Header("Camera placement requirements")]
    public float CameraMinimumDistanceFromWall;

    public float MaximumCameraXRotation;
    public float MaximumCameraYRotation;
    public float MaximumCameraZRotation;
}
