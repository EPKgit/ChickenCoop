using System.Collections;
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
        if (overrider != null && overrider.GetOverridingBlock().HasStat(StatName.Toughness))
        {
            EditorGUILayout.LabelField("Max Health is being set by overrider " + overrider);
            EditorGUILayout.LabelField("Value: " + overrider.GetOverridingBlock().GetValue(StatName.Toughness));
            return;
        }
		if (statBlock != null && statBlock.HasStat(StatName.Toughness))
        {
            EditorGUILayout.LabelField("Max Health is being set by the StatBlock");
			EditorGUILayout.LabelField("Value: " + statBlock.GetValue(StatName.Toughness));
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
            baseHealth.Damage(1, null, null);
        }
    }
}
