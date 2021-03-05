using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PoolManager))]
public class PoolManagerInspector : Editor
{
	private static bool poolFoldout = true;

	private PoolManager poolManager;

	void OnEnable()
	{
		poolManager = target as PoolManager;
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		if( !(EditorApplication.isPlaying) )
		{
			return;
		}
		if(poolFoldout = EditorGUILayout.Foldout(poolFoldout, "Pools"))
		{
			foreach(PoolData pd in poolManager.GetPools())
			{
				EditorGUILayout.LabelField(pd.defaultGO.name + ":" + pd.desiredSize + ":"+pd.currentTTL +"/"+pd.maxTTL);
			}
		}
	}
}
