﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthStation : BaseInteractable
{
	protected override bool CanInteract()
	{
		return true;
	}

	protected override void PerformInteract(GameObject user)
	{
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.INTERACTABLES, "HealthStation");
        Lib.FindUpwardsInTree<IHealable>(user)?.Heal
        (
            HealthChangeData.GetBuilder()
                .Healing(1)
                .BothSources(gameObject)
				.Target(user)
                .Finalize()
        );
	}
}
