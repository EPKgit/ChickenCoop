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


    void OnEnable()
	{
		playerMovement = target as PlayerMovement;
		statBlock = playerMovement.gameObject.GetComponent<StatBlockComponent>();
        overrider = statBlock.GetComponent<IStatBlockInitializer>();
    }

    public override void OnInspectorGUI()
	{
        if (EditorApplication.isPlaying || EditorApplication.isPaused)
        {
            InGameDisplay();
            return;
        }
        if (overrider != null && overrider.GetOverridingBlock().HasStat(StatName.Agility))
        {
            EditorGUILayout.LabelField("MoveSpeed is being set by overrider " + overrider);
            EditorGUILayout.LabelField("Value: " + overrider.GetOverridingBlock().GetValue(StatName.Agility));
            return;
        }
        if (statBlock != null && statBlock.HasStat(StatName.Agility))
        {
            EditorGUILayout.LabelField("MoveSpeed is being set by the StatBlock");
            EditorGUILayout.LabelField("Value: " + statBlock.GetValue(StatName.Agility));
        }
        InGameDisplay();
    }

    void InGameDisplay()
    {
        base.OnInspectorGUI();
    }
}
