using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerClass))]
public class PlayerClassInspector : Editor
{
    private PlayerClass pc;

    private new SerializedProperty name;
    private SerializedProperty abilities;
    private SerializedProperty playerModelPrefab;
    private SerializedProperty stats;

    void OnEnable()
    {
        pc = target as PlayerClass;
        name = serializedObject.FindProperty("name");
        abilities = serializedObject.FindProperty("abilities");
        playerModelPrefab = serializedObject.FindProperty("playerModelPrefab");
        stats = serializedObject.FindProperty("stats");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(name);
        EditorGUILayout.PropertyField(abilities);
        EditorGUILayout.PropertyField(playerModelPrefab);
    
        EditorGUILayout.PropertyField(stats);
    }
}
