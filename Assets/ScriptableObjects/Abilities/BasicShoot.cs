using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicShoot : Ability
{
	public GameObject bulletPrefab;
	public float moveSpeed;
	public float damage;
	public float lifetime = 6.0f;

	public override void Initialize(PlayerAbilities pa)
	{
		PoolManager.instance.AddPoolSize(bulletPrefab, 20, true);
		base.Initialize(pa);
	}

    protected override void UseAbility(Vector2 targetPoint)
	{
        Vector2 direction = GetNormalizedDirectionTowardsTarget(targetPoint);
        direction = Lib.DefaultDirectionCheck(direction);
        direction *= moveSpeed;
		GameObject temp = PoolManager.instance.RequestObject(bulletPrefab);
		temp.GetComponent<Bullet>().Setup
		(
			playerAbilities.transform.position, 
            direction, 
            playerAbilities.gameObject, 
			damage * playerAbilities.stats.GetValue(StatName.DamagePercentage),
            lifetime
		);
		temp.GetComponent<Poolable>().Reset();
	}
}
