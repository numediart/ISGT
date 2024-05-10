using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomsGenerator))]
public class RoomsGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoomsGenerator script = (RoomsGenerator)target;

        if (GUILayout.Button("Room Generation"))
            script.GenerateRooms();

        if (GUILayout.Button("Apply Materials"))
            script.ApplyMaterialsForAllRooms();

        if (GUILayout.Button("Clear Scene"))
            script.ClearScene();
        
      
    }
}
