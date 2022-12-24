using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class BaseInteractable : Poolable
{
	public int priority;

    public PerformableAction toPerform;
	
	void Awake()
	{
		toPerform = new PerformableAction(CanInteract, priority, PerformInteract);
	}

	protected abstract void PerformInteract(GameObject user);

	protected abstract bool CanInteract();

	void OnTriggerEnter2D(Collider2D col)
	{
		PlayerInteraction pi = Lib.FindUpwardsInTree<PlayerInteraction>(col.gameObject);
		if(pi == null)
		{
			return;
		}
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.INTERACTABLES, "registering");
		pi.RegisterAction(toPerform);
	}

	void OnTriggerExit2D(Collider2D col)
	{
		PlayerInteraction pi = Lib.FindUpwardsInTree<PlayerInteraction>(col.gameObject);
		if(pi == null)
		{
			return;
		}
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.INTERACTABLES, "deregistering");
		pi.DeregisterAction(toPerform);
	}
}
