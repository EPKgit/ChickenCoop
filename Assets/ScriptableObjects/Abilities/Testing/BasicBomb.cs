﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicBomb : Ability
{
    public override uint ID => 2;

    public GameObject bombPrefab;
	public float arcSteepness;
	public float arcTime;
    public float damage;
    public float aoe;

    public override void Initialize(PlayerAbilities pa)
	{
		PoolManager.instance.AddPoolSize(bombPrefab, 3, true);
		base.Initialize(pa);
	}

    public override void Cleanup(PlayerAbilities pa)
    {
        PoolManager.instance.RemovePoolSize(bombPrefab, 3);
        base.Cleanup(pa);
    }

    protected override void UseAbility()
	{
        base.UseAbility();
		GameObject temp = PoolManager.instance.RequestObject(bombPrefab);
        Bomb b = temp.GetComponent<Bomb>();
        b.arcSteepness = arcSteepness;
        b.arcTime = (targetingData.inputPoint - (Vector2)playerAbilities.transform.position).magnitude / Range * arcTime; //the arc time decreases the shorter we aim
        b.arcTime = Mathf.Clamp(b.arcTime, 0.5f, arcTime);
        b.Setup
        (
            playerAbilities.transform.position,
            targetingData.inputPoint,
            playerAbilities.gameObject,
            Lib.FindUpwardsInTree<TargetingController>(playerAbilities.gameObject)?.TargetAffiliation ?? Targeting.Affiliation.NONE
		);
        b.damage = damage;
        b.explosionRadius = aoe;
	}
}
