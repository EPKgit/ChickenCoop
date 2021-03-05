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

	void OnEnable()
	{
		ability = target as Ability;
		childFields = new List<FieldInfo>(ability.GetType().GetFields());
		for(int x = childFields.Count - 1; x >= 0; --x)
		{
			if(childFields[x].DeclaringType == typeof(Ability))
			{
				childFields.RemoveAt(x);
			}
		}
		ticking = serializedObject.FindProperty("tickingAbility");
		hasDuration = serializedObject.FindProperty("hasDuration");
		maxDuration = serializedObject.FindProperty("maxDuration");
		pressOnly = serializedObject.FindProperty("pressOnly");
		cost = serializedObject.FindProperty("cost");
		cooldown = serializedObject.FindProperty("maxCooldown");
		icon = serializedObject.FindProperty("icon");
		passive = serializedObject.FindProperty("isPassive");
	}

	private SerializedProperty passive;
	private SerializedProperty ticking;
	private SerializedProperty hasDuration;
	private SerializedProperty maxDuration;
	private SerializedProperty pressOnly;
	private SerializedProperty cost;
	private SerializedProperty cooldown;
	private SerializedProperty icon;
	

	public override void OnInspectorGUI()
	{
		EditorGUILayout.PropertyField(passive);
		if(!ability.isPassive)
		{
			EditorGUILayout.PropertyField(ticking);
			if(ability.tickingAbility)
			{
				EditorGUILayout.PropertyField(hasDuration);
				if(ability.hasDuration)
				{
					EditorGUILayout.PropertyField(maxDuration);
				}
			}
			EditorGUILayout.PropertyField(pressOnly);
			EditorGUILayout.PropertyField(cost);
			EditorGUILayout.PropertyField(cooldown);
			EditorGUILayout.PropertyField(icon);
		}
		foreach(FieldInfo temp in childFields)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty(temp.Name));
		}
		serializedObject.ApplyModifiedProperties();
	}
}
