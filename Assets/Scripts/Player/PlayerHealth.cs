using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : BaseHealth
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Die(GameObject killer = null)
    {
        base.Die();
        gameObject.transform.position = Vector3.zero;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        postDamageNotification += OnDamageTaken;
        shieldAbsorbedDamageNotification += OnShieldAbsorbedDamage;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        postDamageNotification -= OnDamageTaken;
        shieldAbsorbedDamageNotification -= OnShieldAbsorbedDamage;
    }

    void OnShieldAbsorbedDamage(ShieldAbsorbtionData sad)
    {
        StatusEffectManager.instance.ApplyEffect(gameObject, Statuses.StatusEffectType.INVULERNABILITY, 0.5f);
    }

    void OnDamageTaken(HealthChangeData hcd)
    {
        StatusEffectManager.instance.ApplyEffect(gameObject, Statuses.StatusEffectType.INVULERNABILITY, 0.5f);
    }
}
