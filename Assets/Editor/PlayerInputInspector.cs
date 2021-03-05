using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerInput))]
public class PlayerInputInspector : Editor
{
    public static bool debugFoldout = true;

	private PlayerInput playerInput;

	void OnEnable()
	{
		playerInput = target as PlayerInput;
	}

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		if(debugFoldout = EditorGUILayout.Foldout(debugFoldout, "Debug"))
		{
			EditorGUI.indentLevel++;
			playerInput.testingController = EditorGUILayout.Toggle("Controller", playerInput.testingController);
			playerInput.testingMouseAndKeyboard = EditorGUILayout.Toggle("Mouse+KB", playerInput.testingMouseAndKeyboard);
			EditorGUI.indentLevel--;
		}
		EditorGUILayout.LabelField("Player ID:" + playerInput.playerID);
		if(EditorGUI.EndChangeCheck())
		{
			EditorUtility.SetDirty(playerInput);
		}
	}
}
