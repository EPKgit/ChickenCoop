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
            bool dirty = false;
            List<StatName> _keys = new List<StatName>(stats.Keys);
            foreach (var key in _keys)
            {
                float baseValue = stats[key].BaseValue;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(key.ToString());
                float f = EditorGUILayout.DelayedFloatField(baseValue);

                if (GUILayout.Button("-"))
                {
                    stats.Remove(key);
                    dirty = true;
                }

                if (f != baseValue)
                {
                    stats[key].OverwriteBaseValueNoUpdate(f);
                    dirty = true;
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            newKey = (StatName)EditorGUILayout.EnumPopup(newKey);
            if (!stats.ContainsKey(newKey) && GUILayout.Button("+"))
            {
                stats.Add(newKey, new Stat(newKey, 1));
                dirty = true;
            }
            EditorGUILayout.EndHorizontal();

            if(dirty)
            {
                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }
	}
}
