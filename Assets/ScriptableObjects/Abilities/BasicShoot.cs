using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicShoot : Ability, IDamagingAbility
{
	public GameObject bulletPrefab;
	public float moveSpeed;
	public float lifetime = 6.0f;

    float IDamagingAbility.damage
    {
        get
        {
            return _damage;
        }
        set
        {
            _damage = value;
        }
    }
    [SerializeField]
    private float _damage;

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
			_damage * playerAbilities.stats.GetValue(StatName.DamagePercentage),
            lifetime
		);
		temp.GetComponent<Poolable>().Reset();
	}
}
