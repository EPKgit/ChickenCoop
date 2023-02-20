using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class BaseInteractable : Poolable
{
	public uint ID;
	public int priority;

    public PerformableAction toPerform;

	private static uint IDCounter = 0;
	void Awake()
	{
		ID = IDCounter++;
		if(IDCounter == uint.MaxValue)
        {
			IDCounter = 0;
        }
		toPerform = new PerformableAction(gameObject, CanInteract, priority, PerformInteract);
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
		DebugFlags.Log(DebugFlags.FLAGS.INTERACTABLES, "registering");
		pi.RegisterAction(toPerform);
	}

	void OnTriggerExit2D(Collider2D col)
	{
		PlayerInteraction pi = Lib.FindUpwardsInTree<PlayerInteraction>(col.gameObject);
		if(pi == null)
		{
			return;
		}
		DebugFlags.Log(DebugFlags.FLAGS.INTERACTABLES, "deregistering");
		pi.DeregisterAction(toPerform);
	}
}
