﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BaseHealth), true)]
public class BaseHealthInspector : Editor
{
	private BaseHealth baseHealth;
	private StatBlockComponent statBlock;
    private IStatBlockInitializer overrider;

    private SerializedProperty maxHealth;


    void OnEnable()
	{
		baseHealth = target as BaseHealth;
		statBlock = baseHealth.gameObject.GetComponent<StatBlockComponent>();
        overrider = statBlock?.GetComponent<IStatBlockInitializer>();
        maxHealth = serializedObject.FindProperty("_maxHealth");
    }

	public override void OnInspectorGUI()
	{
		if(EditorApplication.isPlaying || EditorApplication.isPaused)
		{
            InGameDisplay();
            return;
        }
        if (overrider != null && overrider.GetOverridingBlock().HasStat(StatName.MaxHealth))
        {
            EditorGUILayout.LabelField("Max Health is being set by overrider " + overrider);
            EditorGUILayout.LabelField("Value: " + overrider.GetOverridingBlock().GetValue(StatName.MaxHealth));
            return;
        }
		if (statBlock != null && statBlock.HasStat(StatName.MaxHealth))
        {
            EditorGUILayout.LabelField("Max Health is being set by the StatBlock");
			EditorGUILayout.LabelField("Value: " + statBlock.GetValue(StatName.MaxHealth));
            return;
		}
        EditorGUILayout.PropertyField(maxHealth);
        serializedObject.ApplyModifiedProperties();
    }

    void InGameDisplay()
    {
        EditorGUILayout.LabelField(string.Format("{0}/{1}", baseHealth.currentHealth, baseHealth.maxHealth));
        if (GUILayout.Button("Take 1 Damage"))
        {
            baseHealth.Damage
            (
                HealthChangeData.GetBuilder()
                    .Damage(1)
                    .BothSources(SingletonHelpers.managerObject)
                    .Target(baseHealth.gameObject)
                    .Finalize()
            );
        }
    }

    public override bool RequiresConstantRepaint()
    {
        return true;
    }
}
