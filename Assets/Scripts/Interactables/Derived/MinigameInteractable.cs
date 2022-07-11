using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameInteractable : BaseInteractable
{
    protected override bool CanDo()
    {
        return true;
    }

    protected override void ToDo(GameObject user)
    {
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.INTERACTABLES, "MinigameInteractable");
        MinigameManager.instance.StartMinigame(Minigame.MinigameType.MAX, OnMinigameFinish, user);
    }

    void OnMinigameFinish(MinigameBase.MinigameStatus status)
	{
		if(status == MinigameBase.MinigameStatus.FINISHED_SUCCESS)
		{
            Debug.Log("SUCCESS");
        }
	}
}
