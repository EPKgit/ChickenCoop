using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerAbilities))]
public class PlayerAbilitiesInspector : Editor
{
	private static bool tickingAbilitiesFoldout = true;
	private static bool intantiatedAbilitiesFoldout = true;

	private PlayerAbilities playerAbilities;

	void OnEnable()
	{
		playerAbilities = target as PlayerAbilities;
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		if(!EditorApplication.isPaused && !EditorApplication.isPlaying)
		{
			return;
		}
		if(intantiatedAbilitiesFoldout = EditorGUILayout.Foldout(intantiatedAbilitiesFoldout, "InstAbilities"))
		{
			if(playerAbilities.GetCurrentlyInstantiatedAbilities() != null)
			{
				foreach(string s in playerAbilities.GetCurrentlyInstantiatedAbilities())
				{
					EditorGUILayout.LabelField(s);
				}
			}
			
		}
		if(tickingAbilitiesFoldout = EditorGUILayout.Foldout(tickingAbilitiesFoldout, "TickingAbilities"))
		{
			if(playerAbilities.GetCurrentlyTickingAbilities() == null)
			{
				foreach(string s in playerAbilities.GetCurrentlyTickingAbilities())
				{
					EditorGUILayout.LabelField(s);
				}
			}
		}
	}
}
