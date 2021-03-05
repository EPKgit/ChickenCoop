using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StatBlock))]
public class StatBlockInspector : Editor
{
	private StatBlock statBlock;
	private float value;

	void OnEnable()
	{
		statBlock = target as StatBlock;
	}	

	public override void OnInspectorGUI()
	{
		if(EditorApplication.isPlaying || EditorApplication.isPaused)
		{
			foreach(StatName s in Enum.GetValues(typeof(StatName)))
			{
				value = statBlock.GetValue(s);
				if(value != -1)
				{
					EditorGUILayout.LabelField(string.Format("{0}:{1}", s.ToString(), value));
				}
			}
		}
		else
		{
			base.OnInspectorGUI();
		}
	}
}
