using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : BaseHealth
{
    protected override void Awake()
    {
        base.Awake();
        postDamageEvent += OnDamage;
    }

    void OnDamage(HealthChangeNotificationData hcnd)
    {
        DamageNumbersManager.instance.CreateNumber(hcnd.value, hcnd.localSource.transform.position);
    }

    protected override void Die(GameObject killer = null)
	{
        base.Die();
        Destroy(gameObject);
    	DeathEffect();
	}

	private void DeathEffect()
    {
	}
}
