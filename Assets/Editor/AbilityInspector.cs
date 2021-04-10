using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Ability), true)]
public class AbilityInspector : Editor
{
	private Ability ability;
	private List<FieldInfo> childFields;

	private SerializedProperty passive;
	private SerializedProperty ticking;
	private SerializedProperty hasDuration;
	private SerializedProperty maxDuration;
	private SerializedProperty cost;
	private SerializedProperty cooldown;
	private SerializedProperty icon;

    private AbilityTargetingData targetingData;

    void OnEnable()
    {
        ability = target as Ability;
        childFields = new List<FieldInfo>(ability.GetType().GetFields());
        for (int x = childFields.Count - 1; x >= 0; --x)
        {
            if (childFields[x].DeclaringType == typeof(Ability))
            {
                childFields.RemoveAt(x);
            }
        }
        ticking = serializedObject.FindProperty("tickingAbility");
        hasDuration = serializedObject.FindProperty("hasDuration");
        maxDuration = serializedObject.FindProperty("maxDuration");
        cost = serializedObject.FindProperty("cost");
        cooldown = serializedObject.FindProperty("maxCooldown");
        icon = serializedObject.FindProperty("icon");
        passive = serializedObject.FindProperty("isPassive");
        targetingData = ability.targetingData;
    }

    public override void OnInspectorGUI()
	{
        EditorGUILayout.PrefixLabel("Ability Data");
        ++EditorGUI.indentLevel;
        EditorGUILayout.PropertyField(icon);
        EditorGUILayout.PropertyField(passive);
        if (!ability.isPassive)
        {
            EditorGUILayout.PropertyField(ticking);
            if (ability.tickingAbility)
            {
                EditorGUILayout.PropertyField(hasDuration);
                if (ability.hasDuration)
                {
                    EditorGUILayout.PropertyField(maxDuration);
                }
            }
            EditorGUILayout.PropertyField(cost);
            EditorGUILayout.PropertyField(cooldown);
            --EditorGUI.indentLevel;

            DoTargetData();
        }
        --EditorGUI.indentLevel;
        EditorGUILayout.Space();
        EditorGUILayout.PrefixLabel("Unique Ability Fields");
        ++EditorGUI.indentLevel;
        foreach (FieldInfo temp in childFields)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty(temp.Name));
		}
        --EditorGUI.indentLevel;
        serializedObject.ApplyModifiedProperties();
	}

    void DoTargetData()
    {
        bool dirty = false;
        EditorGUILayout.Space();
        EditorGUILayout.PrefixLabel("Targeting Data");
        ++EditorGUI.indentLevel;
        LayoutField((a) => { return (AbilityTargetingData.TargetType)EditorGUILayout.EnumPopup(a); }, ref targetingData.targetType, ref dirty);

        if (targetingData.targetType != AbilityTargetingData.TargetType.NONE)
        {
            LayoutField((a) => { return (GameObject)EditorGUILayout.ObjectField("Range Preview Prefab", a, typeof(GameObject), false); }, ref targetingData.rangePreviewPrefab, ref dirty);
            LayoutField((a) => { return EditorGUILayout.DelayedFloatField("Range", a); }, ref targetingData.range, ref dirty);
        }
        switch(targetingData.targetType)
        {
            case AbilityTargetingData.TargetType.LINE_TARGETED:
            {
                LayoutField((a) => { return (GameObject)EditorGUILayout.ObjectField("Line Preview Prefab", a, typeof(GameObject), false); }, ref targetingData.secondaryPreviewPrefab, ref dirty);
                LayoutField((a) => { return EditorGUILayout.DelayedFloatField("Perpindicular Scale", a); }, ref targetingData.previewScale.x, ref dirty);
            }
            break;
            case AbilityTargetingData.TargetType.GROUND_TARGETED:
            {
                LayoutField((a) => { return (GameObject)EditorGUILayout.ObjectField("Target Preview Prefab", a, typeof(GameObject), false); }, ref targetingData.secondaryPreviewPrefab, ref dirty);
                LayoutField((a) => { return EditorGUILayout.Vector3Field("Preview Scale", a); }, ref targetingData.previewScale, ref dirty);
            }
            break;
        }

        if (dirty)
        {
            EditorUtility.SetDirty(target);
        }
    }

    void LayoutField<T>(Func<T, T> action, ref T dataToChange, ref bool dirty)
    {
        T temp = (T)action(dataToChange);
        dirty |= !temp.Equals(dataToChange);
        dataToChange = temp;
    }
}
