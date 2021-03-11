using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StatBlockComponent))]
public class StatBlockInspector : Editor
{
    private static StatName newKey = StatName.Strength;

    private StatBlockComponent statBlock;

    void OnEnable()
	{
		statBlock = target as StatBlockComponent;
    }	

	public override void OnInspectorGUI()
	{
        Dictionary<StatName, Stat> stats = statBlock.GetStatBlock().GetStats();
        if (EditorApplication.isPlaying || EditorApplication.isPaused)
		{
            foreach (var pair in stats)
            {
				EditorGUILayout.LabelField(string.Format("{0}:{1}", pair.Key.ToString(), pair.Value.Value));
			}
		}
		else
		{
            foreach(var pair in stats)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(pair.Key.ToString());
                pair.Value.BaseValue = EditorGUILayout.FloatField(pair.Value.BaseValue);
                if(GUILayout.Button("-"))
                {
                    stats.Remove(pair.Key);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            newKey = (StatName)EditorGUILayout.EnumPopup(newKey);
            if (GUILayout.Button("+") && !stats.ContainsKey(newKey))
            {
                stats.Add(newKey, new Stat(newKey, 1));
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
	}
}
