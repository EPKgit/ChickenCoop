using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageStation : BaseInteractable
{
	protected override bool CanDo()
	{
		return true;
	}

	protected override void ToDo(GameObject user)
	{
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.INTERACTABLES, "DamageStation");
		Lib.FindUpwardsInTree<IDamagable>(user)?.Damage(1, gameObject, gameObject);
	}
}
