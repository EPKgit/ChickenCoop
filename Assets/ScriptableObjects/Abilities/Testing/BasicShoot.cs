using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicShoot : Ability
{
    public override uint ID => 1;

	public GameObject bulletPrefab;
	public float moveSpeed;
	public float lifetime = 6.0f;


    [field: SerializeField]
    public float damage
    {
        get;
        set;
    }

    public override void Initialize(PlayerAbilities pa)
	{
		PoolManager.instance.AddPoolSize(bulletPrefab, 20, true);
		base.Initialize(pa);
	}

    public override void Cleanup(PlayerAbilities pa)
    {
        PoolManager.instance.RemovePoolSize(bulletPrefab, 20);
        base.Cleanup(pa);
    }

    protected override void UseAbility()
	{
        base.UseAbility();
        Vector2 direction = GetNormalizedDirectionTowardsTarget(targetingData.inputPoint);
        direction = Lib.DefaultDirectionCheck(direction);
        direction *= moveSpeed;
		GameObject temp = PoolManager.instance.RequestObject(bulletPrefab);
		temp.GetComponent<Bullet>().Setup
		(
			playerAbilities.transform.position, 
            direction, 
            playerAbilities.gameObject,
            (damage + 1),
            lifetime
		);
        damage = (damage + 1) % 10;
	}
}
