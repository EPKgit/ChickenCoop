﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PiercingSpine_Ability : Ability
{
    public float damage = 1.0f;
    public float projectileSpeed = 10.0f;
    public float projectileLifetime = 0.5f;
    public KnockbackPreset knockbackPreset;
    public GameObject spinePrefab;

    public override void Initialize(PlayerAbilities pa)
	{
		base.Initialize(pa);
    }

    public override void FinishAbility()
    {
        base.FinishAbility();
    }

    protected override void UseAbility()
	{
        base.UseAbility();
        Vector2 direction = GetNormalizedDirectionTowardsTarget(targetingData.inputPoint);
        direction = Lib.DefaultDirectionCheck(direction);
        direction *= projectileSpeed;
        GameObject temp = PoolManager.instance.RequestObject(spinePrefab);
        temp.GetComponent<SpineProjectile_Script>().Setup
        (
            playerAbilities.transform.position,
            direction,
            playerAbilities.gameObject,
            damage,
            projectileLifetime
        );
    }


}
