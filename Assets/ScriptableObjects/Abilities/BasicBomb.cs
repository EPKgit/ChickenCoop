using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicBomb : Ability, IDamagingAbility
{
	public GameObject bombPrefab;
	public float arcSteepness;
	public float arcTime;
    public float aoe;

    public float damage
    {
        get; set;
    }

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
        targetingData.inputPoint = ClampPointWithinRange(targetingData.inputPoint);
		GameObject temp = PoolManager.instance.RequestObject(bombPrefab);
        Bomb b = temp.GetComponent<Bomb>();
        b.arcSteepness = arcSteepness;
        b.arcTime = (targetingData.inputPoint - (Vector2)playerAbilities.transform.position).magnitude / targetingData.range * arcTime; //the arc time decreases the shorter we aim
        b.arcTime = Mathf.Clamp(b.arcTime, 0.5f, arcTime);
        b.Setup
        (
            playerAbilities.transform.position,
            targetingData.inputPoint,
            playerAbilities.gameObject,
            Lib.FindInHierarchy<TargetingController>(playerAbilities.gameObject)?.TargetAffiliation ?? Targeting.Affiliation.NONE
		);
        b.damage = damage;
        b.explosionRadius = aoe;
	}
}
