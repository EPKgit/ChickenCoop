using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerInteraction))]
public class PlayerInteractionInspector : Editor
{
    public static bool interactionListFoldout = true;

	private PlayerInteraction playerInteraction;

	void OnEnable()
	{
		playerInteraction = target as PlayerInteraction;
	}

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		if(interactionListFoldout = EditorGUILayout.Foldout(interactionListFoldout, "InteractionList"))
		{
			EditorGUI.indentLevel++;
			var actions = playerInteraction.GetCurrentInteractions();
			if (actions == null || actions.Count <= 0)
			{
				EditorGUILayout.LabelField("None");
			}
			else
			{
				DisplayActions(actions);
			}
			EditorGUI.indentLevel--;
		}
		if(EditorGUI.EndChangeCheck())
		{
			EditorUtility.SetDirty(playerInteraction);
		}
	}

	void DisplayActions(List<PerformableAction> actions)
    {
        actions.Sort((a, b) =>
        {
			if (a.priority == b.priority)
			{
				float aMag = (a.source.transform.position - playerInteraction.transform.position).magnitude;
				float bMag = (b.source.transform.position - playerInteraction.transform.position).magnitude;
				return (int)((aMag - bMag)*10000);
			}
			return a.priority - b.priority;
        });
		Dictionary<int, bool> repeatedPriority = new Dictionary<int, bool>();
		foreach (var action in actions)
		{
			if (repeatedPriority.ContainsKey(action.priority))
			{
				repeatedPriority[action.priority] = true;
			}
			else
			{
				repeatedPriority[action.priority] = false;
			}
		}
		foreach (var action in actions)
		{
			if (repeatedPriority.ContainsKey(action.priority) && repeatedPriority[action.priority])
			{
				float dist = (action.source.transform.position - playerInteraction.transform.position).magnitude;
				EditorGUILayout.LabelField(string.Format("Priority {0} : Dist {1:0.00} : {2}", action.priority, dist, action.source.name));
			}
			else
			{
				EditorGUILayout.LabelField(string.Format("Priority {0}:{1}", action.priority, action.source.name));
			}
		}
	}
}
