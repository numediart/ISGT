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
        
        serializedObject.Update();
        
        RoomsGenerator script = (RoomsGenerator)target;
        
        if (script._manualSeeds)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_roomSeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_openingSeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_objectSeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_databaseSeed"));
        }

        serializedObject.ApplyModifiedProperties();
        
        

        if (GUILayout.Button("Room Generation"))
            script.GenerateRooms();

        if (GUILayout.Button("Apply Materials"))
            script.ApplyMaterialsForAllRooms();

        if (GUILayout.Button("Clear Scene"))
            script.ClearScene();
        
      
    }
}
