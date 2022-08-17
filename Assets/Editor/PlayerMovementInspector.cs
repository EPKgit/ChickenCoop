using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerMovement))]
public class PlayerMovementInspector : Editor
{
	private PlayerMovement playerMovement;
	private StatBlockComponent statBlock;
    private IStatBlockInitializer overrider;

    private SerializedProperty getsKnockbackInvuln;

    void OnEnable()
	{
		playerMovement = target as PlayerMovement;
		statBlock = playerMovement.gameObject.GetComponent<StatBlockComponent>();
        overrider = statBlock.GetComponent<IStatBlockInitializer>();

        getsKnockbackInvuln = serializedObject.FindProperty("getsKnockbackInvuln");
    }

    public override void OnInspectorGUI()
	{
        if (EditorApplication.isPlaying || EditorApplication.isPaused)
        {
            InGameDisplay();
            return;
        }
        if (overrider != null && overrider.GetOverridingBlock().HasStat(StatName.MovementSpeed))
        {
            EditorGUILayout.LabelField("MoveSpeed is being set by overrider " + overrider);
            EditorGUILayout.LabelField("Value: " + overrider.GetOverridingBlock().GetValue(StatName.MovementSpeed));
        }
        else if (statBlock != null && statBlock.HasStat(StatName.MovementSpeed))
        {
            EditorGUILayout.LabelField("MoveSpeed is being set by the StatBlock");
            EditorGUILayout.LabelField("Value: " + statBlock.GetValue(StatName.MovementSpeed));
        }
        serializedObject.Update();
        EditorGUILayout.PropertyField(getsKnockbackInvuln);
        serializedObject.ApplyModifiedProperties();
    }

    void InGameDisplay()
    {
        if(statBlock != null && statBlock.HasStat(StatName.MovementSpeed))
        {
            EditorGUILayout.LabelField("MoveSpeed is being set by the StatBlock");
            EditorGUILayout.LabelField("Value: " + statBlock.GetValue(StatName.MovementSpeed));
            serializedObject.Update();
            EditorGUILayout.PropertyField(getsKnockbackInvuln);
            serializedObject.ApplyModifiedProperties();
        }
        else
        {
            base.OnInspectorGUI();
        }
    }
}
