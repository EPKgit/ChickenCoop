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
    private IStatBlockInitializer overrider;

    void OnEnable()
	{
		statBlock = target as StatBlockComponent;
        overrider = statBlock.GetComponent<IStatBlockInitializer>();
    }	

	public override void OnInspectorGUI()
	{
        if(overrider != null && !EditorApplication.isPlaying && !EditorApplication.isPaused)
        {
            EditorGUILayout.LabelField("Is being overriden by " + overrider.GetType().Name);
            var overridenStats = overrider.GetOverridingBlock().GetStats();
            foreach (var pair in overridenStats)
            {
                EditorGUILayout.LabelField(string.Format("{0}:{1}", pair.Key.ToString(), pair.Value.Value));
            }
            return;
        }
        Dictionary<StatName, Stat> stats = statBlock.GetStatBlock().GetStats();
        if (EditorApplication.isPlaying || EditorApplication.isPaused)
		{
            foreach (var pair in stats)
            {
				EditorGUILayout.LabelField(string.Format("{0}:{1}", pair.Key.ToString(), pair.Value.Value));
			}
            return;
		}
        serializedObject.Update();

        bool dirty = false;
        List<StatName> _keys = new List<StatName>(stats.Keys);
        foreach (var key in _keys)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(key.ToString());
            EditorGUI.BeginChangeCheck();
            float f = EditorGUILayout.DelayedFloatField(stats[key].BaseValue);
            if(EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "changed base value");
                stats[key].OverwriteBaseValueNoUpdate(f);
                dirty = true;
            }
            if (GUILayout.Button("-"))
            {
                Undo.RecordObject(target, "removed key");
                stats.Remove(key);
                dirty = true;
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
        newKey = (StatName)EditorGUILayout.EnumPopup(newKey);
        if (!stats.ContainsKey(newKey) && GUILayout.Button("+"))
        {
            Undo.RecordObject(target, "added key");
            stats.Add(newKey, new Stat(newKey, 1));
            dirty = true;
        }
        EditorGUILayout.EndHorizontal();

        if (dirty)
        {
            EditorUtility.SetDirty(target);
        }

        serializedObject.ApplyModifiedProperties();
	}
}
