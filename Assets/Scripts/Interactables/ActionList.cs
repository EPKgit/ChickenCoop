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

	public PerformableAction GetFirstPerformableAction()
	{
		if(actions.Count == 0)
		{
            return null;
        }
        if (actions.Max.IsPerformable?.Invoke() ?? false)
        {
            return actions.Max;
        }
        var iter = actions.Reverse();
		foreach(var action in iter)
		{
			if(action.IsPerformable?.Invoke() ?? false)
			{
                return action;
            }
		}
        return null;
    }
}
