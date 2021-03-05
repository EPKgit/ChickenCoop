using System.Collections;
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
		return actionList.GetAction()?.IsPerformable?.Invoke() ?? false;
	}

	/// <summary>
	/// Attempts to perform the action
	/// </summary>
	/// <returns>Returns true if the action was performed, false if it wasn't</returns>
	public bool AttemptPerform()
	{
		if( (!actionList.GetAction()?.IsPerformable?.Invoke() ?? true) )
		{
			return false;
		}
		actionList.GetAction().Action(gameObject);
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
}
