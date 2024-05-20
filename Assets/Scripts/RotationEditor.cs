using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CamrotationTest))]
public class RoomsGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        serializedObject.Update();
        
        CamrotationTest script = (CamrotationTest)target;
        
        serializedObject.ApplyModifiedProperties();
        
        

        if (GUILayout.Button("Rotate Camera"))
            script.RotateCamera();
    }
}