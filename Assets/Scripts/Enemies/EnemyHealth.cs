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

    void OnDamage(HealthChangeData hcd)
    {
        Vector3 location;
        if (hcd.DamageLocation != null)
        {
            location = hcd.DamageLocation();
        }
        else if (hcd.Target != null)
        {
            location = hcd.Target.transform.position;
        }
        else
        {
            Debug.LogError("ERROR: Trying to spawn damage numbers without proper location setup");
            location = transform.position;
        }
        DamageNumbersManager.instance.CreateNumber(-hcd.Delta, location, 0.3f);
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
