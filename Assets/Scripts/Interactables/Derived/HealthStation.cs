using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthStation : BaseInteractable
{
	protected override bool CanDo()
	{
		return true;
	}

	protected override void ToDo(GameObject user)
	{
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.INTERACTABLES, "HealthStation");
		Lib.FindInHierarchy<IHealable>(user)?.Heal(1, gameObject, gameObject);
	}
}
