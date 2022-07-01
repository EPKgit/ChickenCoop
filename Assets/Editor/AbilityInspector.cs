using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Targeting;

[CustomEditor(typeof(Ability), true)]
public class AbilityInspector : Editor
{
	private Ability ability;
	private List<FieldInfo> childFields;

	private SerializedProperty ID;
	private SerializedProperty passive;
	private SerializedProperty ticking;
	private SerializedProperty hasDuration;
	private SerializedProperty maxDuration;
	private SerializedProperty cost;
	private SerializedProperty cooldown;
	private SerializedProperty icon;
	private SerializedProperty abilityTags;
	private SerializedProperty tagsToBlock;
	private SerializedProperty tagsToApply;

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
        ID = serializedObject.FindProperty("ID");
        ticking = serializedObject.FindProperty("tickingAbility");
        hasDuration = serializedObject.FindProperty("hasDuration");
        maxDuration = serializedObject.FindProperty("maxDuration");
        cost = serializedObject.FindProperty("cost");
        cooldown = serializedObject.FindProperty("maxCooldown");
        icon = serializedObject.FindProperty("icon");
        passive = serializedObject.FindProperty("isPassive");
        abilityTags = serializedObject.FindProperty("abilityTags");
        tagsToBlock = serializedObject.FindProperty("tagsToBlock");
        tagsToApply = serializedObject.FindProperty("tagsToApply");
        targetingData = ability.targetingData;
    }

    public override void OnInspectorGUI()
	{
        EditorGUILayout.PrefixLabel("Ability Data");
        ++EditorGUI.indentLevel;
        EditorGUILayout.PropertyField(ID);
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
                    PotentiallyOverrideProperty(ID.intValue, maxDuration);
                }
            }
            EditorGUILayout.PropertyField(cost);
            PotentiallyOverrideProperty(ID.intValue, cooldown);
            
            --EditorGUI.indentLevel;

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            DoTargetData();
        }
        --EditorGUI.indentLevel;
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.PropertyField(abilityTags);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.PropertyField(tagsToBlock);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.PropertyField(tagsToApply);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.PrefixLabel("Unique Ability Fields");

        ++EditorGUI.indentLevel;
        foreach (FieldInfo temp in childFields)
		{
            PotentiallyOverrideProperty(ID.intValue, serializedObject.FindProperty(temp.Name));
		}
        --EditorGUI.indentLevel;
        serializedObject.ApplyModifiedProperties();
	}

    void PotentiallyOverrideProperty(int ID, SerializedProperty prop)
    {
        PotentiallyOverrideProperty((uint)ID, prop);
    }
    void PotentiallyOverrideProperty(uint ID, SerializedProperty prop)
    {
        if (AbilityDataXMLParser.instance.HasFieldInEntry(ID, prop.propertyPath))
        {
            EditorGUILayout.LabelField(string.Format("{0} is overriden in XML", prop.propertyPath));
        }
        else
        {
            EditorGUILayout.PropertyField(prop);
        }
    }

    private static bool customTargetingPrefabs = false;

    void DoTargetData()
    {
        bool dirty = false;
        EditorGUILayout.PrefixLabel("Targeting Data");
        ++EditorGUI.indentLevel;
        LayoutField((a) => { return (AbilityTargetingData.TargetType)EditorGUILayout.EnumPopup(a); }, ref targetingData.targetType, ref dirty);

        customTargetingPrefabs = EditorGUILayout.Toggle("Custom Prefab Targeting", customTargetingPrefabs);

        if (targetingData.targetType != AbilityTargetingData.TargetType.NONE)
        {
            if(customTargetingPrefabs) LayoutField((a) => { return (GameObject)EditorGUILayout.ObjectField("Range Preview Prefab", a, typeof(GameObject), false); }, ref targetingData.rangePreviewPrefab, ref dirty);
            LayoutField((a) => { return EditorGUILayout.DelayedFloatField("Range", a); }, ref targetingData.range, ref dirty);
        }
        switch(targetingData.targetType)
        {
            case AbilityTargetingData.TargetType.LINE_TARGETED:
            {
                if (customTargetingPrefabs) LayoutField((a) => { return (GameObject)EditorGUILayout.ObjectField("Line Preview Prefab", a, typeof(GameObject), false); }, ref targetingData.secondaryPreviewPrefab, ref dirty);
                LayoutField((a) => { return EditorGUILayout.DelayedFloatField("Perpindicular Scale", a); }, ref targetingData.previewScale.x, ref dirty);
            }
            break;
            case AbilityTargetingData.TargetType.GROUND_TARGETED:
            {
                if (customTargetingPrefabs) LayoutField((a) => { return (GameObject)EditorGUILayout.ObjectField("Target Preview Prefab", a, typeof(GameObject), false); }, ref targetingData.secondaryPreviewPrefab, ref dirty);
                LayoutField((a) => { return EditorGUILayout.Vector3Field("Preview Scale", a); }, ref targetingData.previewScale, ref dirty);
            }
            break;
            case AbilityTargetingData.TargetType.ENTITY_TARGETED:
            {
                if (customTargetingPrefabs) LayoutField((a) => { return (GameObject)EditorGUILayout.ObjectField("Crosshair Preview Prefab", a, typeof(GameObject), false); }, ref targetingData.secondaryPreviewPrefab, ref dirty);
                LayoutField((a) => { return (Targeting.Affiliation)EditorGUILayout.EnumFlagsField("Affiliation", a); }, ref targetingData.affiliation, ref dirty);
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
        if(temp != null)
        {
            dirty |= !temp.Equals(dataToChange);
        }
        else
        {
            dirty = dataToChange != null;
        }
        dataToChange = temp;
    }
}
