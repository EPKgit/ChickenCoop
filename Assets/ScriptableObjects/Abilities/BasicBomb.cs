using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicBomb : Ability
{
	public GameObject bombPrefab;
	public float arcSteepness;
	public float arcTime;

	public override void Initialize(PlayerAbilities pa)
	{
		PoolManager.instance.AddPoolSize(bombPrefab, 20, true);
		base.Initialize(pa);
	}

    protected override void UseAbility(Vector2 targetPoint)
	{
        targetPoint = ClampPointWithinRange(targetPoint);
		GameObject temp = PoolManager.instance.RequestObject(bombPrefab);
        Bomb b = temp.GetComponent<Bomb>();
        b.arcSteepness = arcSteepness;
        b.arcTime = (targetPoint - (Vector2)playerAbilities.transform.position).magnitude / targetingData.range * arcTime; //the arc time decreases the shorter we aim
		b.Setup
		(
			playerAbilities.transform.position, 
            targetPoint,
            playerAbilities.gameObject
		);
		temp.GetComponent<Poolable>().Reset();
	}
}
