﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageStation : BaseInteractable
{
	protected override bool CanInteract()
	{
		return true;
	}

	protected override void PerformInteract(GameObject user)
	{
		DebugFlags.Log(DebugFlags.Flags.INTERACTABLES, "DamageStation");
		Lib.FindUpwardsInTree<IDamagable>(user)?.Damage
		(
			HealthChangeData.GetBuilder()
				.Damage(1)
				.BothSources(gameObject)
				.Target(user)
                .Finalize()
		);
	}
}
