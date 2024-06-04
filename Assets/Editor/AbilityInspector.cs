using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Targeting;
using System.Linq;

[CustomEditor(typeof(Ability), true)]
public class AbilityInspector : Editor
{
	private Ability ability;
	private List<FieldInfo> childFields;

	//private SerializedProperty ID;
	private SerializedProperty passive;
	private SerializedProperty ticking;
	private SerializedProperty hasDuration;
	private SerializedProperty maxDuration;
	private SerializedProperty cost;
	private SerializedProperty cooldown;
	private SerializedProperty numberTimesRecastable;
	private SerializedProperty recastWindow;
	private SerializedProperty icon;
	private SerializedProperty abilityTags;
	private SerializedProperty tagsToBlock;
	private SerializedProperty tagsToApply;
	//private SerializedProperty targetingDataArray;

    private const float LABEL_RATIO = 0.4f;

    void OnEnable()
    {
        ability = target as Ability;
        childFields = new List<FieldInfo>(ability.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public));
        for (int x = childFields.Count - 1; x >= 0; --x)
        {
            if (childFields[x].DeclaringType == typeof(Ability))
            {
                childFields.RemoveAt(x);
            }
        }
        //ID = serializedObject.FindProperty("ID");
        ticking = serializedObject.FindProperty("_tickingAbilitySerialized");
        hasDuration = serializedObject.FindProperty("hasDuration");
        maxDuration = serializedObject.FindProperty("maxDuration");
        cost = serializedObject.FindProperty("cost");
        cooldown = serializedObject.FindProperty("maxCooldown");
        icon = serializedObject.FindProperty("icon");
        passive = serializedObject.FindProperty("isPassive");
        abilityTags = serializedObject.FindProperty("abilityTags");
        tagsToBlock = serializedObject.FindProperty("tagsToBlock");
        tagsToApply = serializedObject.FindProperty("tagsToApply");
        numberTimesRecastable = serializedObject.FindProperty("numberTimesRecastable");
        recastWindow = serializedObject.FindProperty("recastWindow");
        //targetingDataArray = serializedObject.FindProperty("_targetingData");
    }

    public override void OnInspectorGUI()
	{
        EditorGUILayout.PrefixLabel("Ability Data");
        //++EditorGUI.indentLevel;
        //var prevLabelWidth = EditorGUIUtility.labelWidth;
        //EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth * LABEL_RATIO;
        //EditorGUILayout.LabelField(string.Format("{0}", ability.ID));
        //EditorGUILayout.PropertyField(icon);
        //EditorGUILayout.PropertyField(passive);
        //if (!ability.isPassive)
        //{
        //    EditorGUILayout.PropertyField(ticking);
        //    if (ability.IsTickingAbility)
        //    {
        //        PotentiallyOverrideProperty(maxDuration);
        //    }
        //    EditorGUILayout.PropertyField(cost);
        //    PotentiallyOverrideProperty(cooldown);

        //    PotentiallyOverrideProperty(numberTimesRecastable);
        //    if (numberTimesRecastable.intValue != 0)
        //    {
        //        PotentiallyOverrideProperty(recastWindow);
        //    }
        //    //DoTargetData();
        //}
        //EditorGUIUtility.labelWidth = prevLabelWidth;
        //--EditorGUI.indentLevel;

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
            if(temp.IsNotSerialized)
            {
                continue;
            }
            System.Attribute[] attributes = System.Attribute.GetCustomAttributes(temp);
            foreach(Attribute attribute in attributes)
            {
                if(attribute is HideInInspector)
                {
                    continue;
                }
            }
            PotentiallyOverrideProperty(temp.Name);
		}
        --EditorGUI.indentLevel;
        serializedObject.ApplyModifiedProperties();
	}

    void PotentiallyOverrideProperty(string name)
    {
        var serializedProperty = serializedObject.FindProperty(name);
        if(serializedProperty == null)
        {
            Debug.LogError(string.Format("ERROR: SERIALIZED PROPERTY \"{0}\" COULD NOT BE FOUND FOR ABILITY WITH ID:{1}", name, ability.ID));
            return;
        }
        PotentiallyOverrideProperty(serializedProperty);
    }

    void PotentiallyOverrideProperty(SerializedProperty prop)
    {
        string entry = AbilityDataXMLParser.instance.HasFieldInEntry(ability.ID, prop.propertyPath);
        if (entry != "")
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(string.Format("{0}", prop.displayName), GUILayout.Width(EditorGUIUtility.currentViewWidth * LABEL_RATIO - (EditorGUI.indentLevel * 15)));
            EditorGUILayout.LabelField(string.Format("{0}", entry));
            GUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.PropertyField(prop);
        }
    }
    static int delIndex = 0;

    //void DoTargetData()
    //{
    //    --EditorGUI.indentLevel;
    //    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    //    EditorGUILayout.PrefixLabel("Targeting Data");
    //    EditorGUILayout.BeginHorizontal();
    //    int n = targetingDataArray.arraySize;
    //    delIndex = EditorGUILayout.DelayedIntField(delIndex);
    //    if(GUILayout.Button("-") && delIndex < n)
    //    {
    //        targetingDataArray.DeleteArrayElementAtIndex(delIndex);
    //        return;
    //    }
    //    if (GUILayout.Button("+"))
    //    {
    //        targetingDataArray.InsertArrayElementAtIndex(n);
    //    }
    //    EditorGUILayout.EndHorizontal();
    //    ++EditorGUI.indentLevel;
    //    for (int x = 0; x < n; ++x)
    //    {
    //        EditorGUILayout.PropertyField(targetingDataArray.GetArrayElementAtIndex(x), new GUIContent("Targeting Data " + x));
    //    }
    //    --EditorGUI.indentLevel;
    //    ++EditorGUI.indentLevel;
    //}

}
