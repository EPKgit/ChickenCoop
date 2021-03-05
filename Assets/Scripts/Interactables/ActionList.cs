using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionList
{
	private SortedSet<PerformableAction> actions;

	public ActionList()
	{
		actions = new SortedSet<PerformableAction>(new PerformableActionComparator());
	}

	public void AddAction(PerformableAction pa)
	{
		actions.Add(pa);
	}

	public void RemoveAction(PerformableAction pa)
	{
		actions.Remove(pa);
	}

	public PerformableAction GetAction()
	{
		return actions.Max;
	}	
}
