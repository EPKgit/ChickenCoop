﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
	private ActionList actionList;

	void Awake()
	{
		actionList = new ActionList();
	}

	//potential highlights or particles on the interactible item that is chosen
	void Update()
	{

	}

	public bool CanPerform()
	{
		return actionList.GetFirstPerformableAction()?.isPerformable?.Invoke() ?? false;
	}

	/// <summary>
	/// Attempts to perform the action
	/// </summary>
	/// <returns>Returns true if the action was performed, false if it wasn't</returns>
	public bool AttemptPerform()
	{
        var firstPerformableAction = actionList.GetFirstPerformableAction();
        if((!firstPerformableAction?.isPerformable?.Invoke() ?? true))
		{
			return false;
		}
        firstPerformableAction?.action(gameObject);
		return true;
	}

	public void RegisterAction(PerformableAction pa)
	{
		actionList.AddAction(pa);
	}

	public void DeregisterAction(PerformableAction pa)
	{
		actionList.RemoveAction(pa);
	}

#if UNITY_EDITOR
	public List<PerformableAction> GetCurrentInteractions()
    {
		return actionList?.GetCurrentInteractions();
    }
#endif
}
