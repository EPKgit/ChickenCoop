using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Statuses;

[CustomEditor(typeof(StatusEffectManager))]
public class StatusEffectManagerInspector : Editor
{
	private static bool iconFoldout = false;
    private static string[] effectTypeNames;


    private SerializedProperty effectBarPrefab;
    private SerializedProperty effectPopupPrefab;
    private SerializedProperty effectSprites;
    private StatusEffectManager effectManager;

	void OnEnable()
	{
        effectManager = target as StatusEffectManager;
        effectBarPrefab = serializedObject.FindProperty("effectBarPrefab");
        effectPopupPrefab = serializedObject.FindProperty("effectPopupPrefab");

        effectTypeNames = Enum.GetNames(typeof(StatusEffectType));
        effectTypeNames = effectTypeNames.Take(effectTypeNames.Length - 1).ToArray(); //cut off last elem (MAX)

        effectSprites = serializedObject.FindProperty("effectSprites");
        if (effectSprites.arraySize != (int)StatusEffectType.MAX)
        {
            Debug.LogError("ERROR STATUS EFFECT ICON ARRAY SIZES OUT OF SYNC");
            Array.Resize(ref effectManager.effectSprites, (int)StatusEffectType.MAX);
        }
    }

	public override void OnInspectorGUI()
	{
        serializedObject.Update();
        EditorGUILayout.PropertyField(effectBarPrefab);
        EditorGUILayout.PropertyField(effectPopupPrefab);
        if(iconFoldout = EditorGUILayout.Foldout(iconFoldout, "Icons"))
		{
            int index = 0;
            foreach (string s in effectTypeNames)
            {
                EditorGUILayout.PropertyField(effectSprites.GetArrayElementAtIndex(index), new GUIContent(s));
                index++;
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
