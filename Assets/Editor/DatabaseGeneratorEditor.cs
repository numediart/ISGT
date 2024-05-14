using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DatabaseGenerator))]
public class DatabaseGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        serializedObject.Update();
        
        DatabaseGenerator script = (DatabaseGenerator)target;
        
        if (script.manualScreenShots)
        {
            //Hide or show lists
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_cameraPositions"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_cameraRotations"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}