using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StatBlockComponent))]
public class StatBlockComponentInspector : Editor
{
    private StatBlockComponent statBlockComponent;
    private IStatBlockInitializer overrider;
    private SerializedProperty statsProp;

    void OnEnable()
	{
		statBlockComponent = target as StatBlockComponent;
        overrider = statBlockComponent.GetComponent<IStatBlockInitializer>();
        statsProp = serializedObject.FindProperty("stats");
    }	

	public override void OnInspectorGUI()
	{
        if(overrider != null && !EditorApplication.isPlaying && !EditorApplication.isPaused)
        {
            EditorGUILayout.LabelField("Is being overriden by " + overrider.GetType().Name);
            var overridenStats = overrider.GetOverridingBlock().GetStats();
            foreach (var pair in overridenStats)
            {
                if(pair.Key == StatName.MAX)
                {
                    continue;
                }
                EditorGUILayout.LabelField(string.Format("{0}:{1}", pair.Key.ToString(), pair.Value.Value));
            }
            return;
        }
        Dictionary<StatName, Stat> stats = statBlockComponent.GetStatBlock().GetStats();
        if (EditorApplication.isPlaying || EditorApplication.isPaused)
		{
            foreach (var pair in stats)
            {
                if (pair.Key == StatName.MAX)
                {
                    continue;
                }
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(string.Format("{0}:{1}", pair.Key.ToString(), pair.Value.Value.ToString()));
                EditorGUI.BeginChangeCheck();
                float f = EditorGUILayout.DelayedFloatField(pair.Value.BaseValue);
                if(EditorGUI.EndChangeCheck()) 
                {
                    pair.Value.BaseValue = f;
                }
                EditorGUILayout.EndHorizontal();
            }
            return;
		}

        serializedObject.Update();
        EditorGUILayout.PropertyField(statsProp);
        serializedObject.ApplyModifiedProperties();
    }
}
