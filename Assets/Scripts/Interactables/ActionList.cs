using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformableActionComparator : IComparer<int>
{
	public int Compare(int p1, int p2)
	{
		return p1 - p2;
	}
}

public class ActionList
{
	private List<PerformableAction> actions;

	public ActionList()
	{
		actions = new List<PerformableAction>();
	}

	public void AddAction(PerformableAction pa)
	{
		actions.Add(pa);
	}

	public void RemoveAction(PerformableAction pa)
	{
		actions.Remove(pa);
	}

	public PerformableAction GetFirstPerformableAction(GameObject interactor = null)
	{
		if(actions.Count == 0)
		{
            return null;
        }
		PerformableAction candidate = null;
		foreach (var action in actions)
        {
			if (!(action.isPerformable?.Invoke() ?? false)) //we only want to check if it's a better candidate if we can perform it
            {
				continue;
            }
			if (interactor != null && action.priority == candidate.priority)
            {
				float candidateSquareMag = (candidate.source.transform.position - interactor.transform.position).sqrMagnitude;
				float currSquareMag = (action.source.transform.position - interactor.transform.position).sqrMagnitude;
				if(currSquareMag < candidateSquareMag)
                {
					candidate = action;
                }
            }
            else if(action.priority > (candidate?.priority ?? int.MinValue))
			{
				candidate = action;
			}
        }
        return candidate;
    }

#if UNITY_EDITOR
	public List<PerformableAction> GetCurrentInteractions()
    {
		return new List<PerformableAction>(actions);
    }
#endif
}
