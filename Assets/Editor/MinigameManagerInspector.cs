using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Minigame;

[CustomEditor(typeof(MinigameManager))]
public class MinigameManagerInspector : Editor
{
	private static bool prefabFoldout = false;
    private static string[] minigameNames;

    private SerializedProperty minigamePrefabs;
    private MinigameManager minigameManager;

	void OnEnable()
	{
        minigameManager = target as MinigameManager;

        minigameNames = Enum.GetNames(typeof(MinigameType));
        minigameNames = minigameNames.Take(minigameNames.Length - 1).ToArray(); //cut off last elem (MAX)        

        minigamePrefabs = serializedObject.FindProperty("minigamePrefabs");
        if (minigamePrefabs.arraySize != (int)MinigameType.MAX)
        {
            Debug.LogError("ERROR MINIGAME PREFAB ARRAY SIZES OUT OF SYNC");
            Array.Resize(ref minigameManager.minigamePrefabs, (int)MinigameType.MAX);
        }
    }

	public override void OnInspectorGUI()
	{
        serializedObject.Update();
        if(prefabFoldout = EditorGUILayout.Foldout(prefabFoldout, "Minigame Prefabs"))
		{
            int index = 0;
            foreach (string s in minigameNames)
            {
                EditorGUILayout.PropertyField(minigamePrefabs.GetArrayElementAtIndex(index), new GUIContent(s));
                index++;
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
