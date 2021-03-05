using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerMovement))]
public class PlayerMovementInspector : Editor
{
	private PlayerMovement playerMovement;
	private StatBlock statBlock;

	void OnEnable()
	{
		playerMovement = target as PlayerMovement;
		statBlock = playerMovement.gameObject.GetComponent<StatBlock>();
	}

	public override void OnInspectorGUI()
	{
		if(statBlock == null || !statBlock.HasStat(StatName.Agility))
		{
			base.OnInspectorGUI();
		}
		else
		{
			EditorGUILayout.LabelField("MoveSpeed is being set by the StatBlock");
			EditorGUILayout.LabelField("Value: " + statBlock.GetValue(StatName.Agility));
			EditorGUILayout.LabelField(string.Format("({0},{1})", playerMovement.direction.x, playerMovement.direction.y));
		}
	}
}
