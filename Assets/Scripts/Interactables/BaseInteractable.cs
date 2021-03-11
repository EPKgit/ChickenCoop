using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class BaseInteractable : MonoBehaviour
{
	public int priority;

    public PerformableAction toPerform;
	
	void Awake()
	{
		toPerform = new PerformableAction(CanDo, priority, ToDo);
	}

	protected abstract void ToDo(GameObject user);

	protected abstract bool CanDo();

	void OnTriggerEnter2D(Collider2D col)
	{
		PlayerInteraction pi = Lib.FindInHierarchy<PlayerInteraction>(col.gameObject);
		if(pi == null)
		{
			return;
		}
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.INTERACTABLES, "registering");
		pi.RegisterAction(toPerform);
	}

	void OnTriggerExit2D(Collider2D col)
	{
		PlayerInteraction pi = Lib.FindInHierarchy<PlayerInteraction>(col.gameObject);
		if(pi == null)
		{
			return;
		}
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.INTERACTABLES, "deregistering");
		pi.DeregisterAction(toPerform);
	}
}
