using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/ObjectsGenerationScriptableObject", order = 2)]
public class ObjectsGenerationScriptableObject : ScriptableObject  
{
    public List<GameObject> Objects = new List<GameObject>();
    public int ObjectsLayerIndex;

    [Header("Requirements")]
    public float MinimumDistanceBetweenObjects;
    public int ObjectsMinimumNumberPerRoom;
    public int ObjectsMaximumNumberPerRoom;
    [HideInInspector] public int MaximumNumberOfObjectPlacementAttempts = 10;
    [HideInInspector] public int MaximumNumberOfObjectsToTryToFitIn = 10;

    [Header("AI Model Use")]
    public string ObjectsLayoutJSONFilePath;
}
