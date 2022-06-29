using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyManager))]
public class EnemyManagerInspector : Editor
{
	private static bool[] foldouts;
	private static string[] enemyTypeNames;

	private SerializedProperty enemySpawnDataProp;


	private EnemyManager enemyManager;

	void OnEnable()
	{
		enemyManager = target as EnemyManager;
		enemyTypeNames = Enum.GetNames(typeof(EnemyType));
		enemyTypeNames = enemyTypeNames.Take(enemyTypeNames.Length - 1).ToArray(); //cut off last elem (MAX)
		foldouts = new bool[enemyTypeNames.Length];
		for(int x = 0; x < foldouts.Length; ++x)
		{
			foldouts[x] = false;
		}
        enemySpawnDataProp = serializedObject.FindProperty("enemySpawnData");
		if(enemySpawnDataProp.arraySize != (int)EnemyType.MAX)
		{
			Debug.LogError("ERROR ENEMY MANAGER ARRAY SIZES OUT OF SYNC");
            Array.Resize(ref enemyManager.enemySpawnData, (int)EnemyType.MAX);
		}
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		int index = 0;
		foreach(string s in enemyTypeNames)
		{
			EditorGUILayout.PropertyField(enemySpawnDataProp.GetArrayElementAtIndex(index), new GUIContent(s));
			index++;
		}
		serializedObject.ApplyModifiedProperties();
	}
}
