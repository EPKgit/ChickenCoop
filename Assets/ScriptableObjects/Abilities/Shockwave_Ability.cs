using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shockwave_Ability : Ability
{
    public GameObject shockwavePrefab;

    public float damage;
    public float stunDuration;
    public float lifetime;
    public float thickness;

    public override string GetTooltip()
    {
        return string.Format(tooltipDescription, damage);
    }

    public override void Initialize(PlayerAbilities pa)
	{
		PoolManager.instance.AddPoolSize(shockwavePrefab, 3, true);
		base.Initialize(pa);
	}

    public override void Cleanup(PlayerAbilities pa)
    {
        PoolManager.instance.RemovePoolSize(shockwavePrefab, 3);
        base.Cleanup(pa);
    }

    protected override void UseAbility()
	{
        base.UseAbility();
		GameObject temp = PoolManager.instance.RequestObject(shockwavePrefab);
		temp.GetComponent<Shockwave>().Setup
		(
			playerAbilities.transform.position,
            lifetime,
            thickness,
            aoe,
            playerAbilities.gameObject,
            (IDamagable i) =>
            {
                i.Damage(damage, temp, playerAbilities.gameObject, PresetKnockbackData.GetKnockbackPreset(KnockbackPreset.BIG));
                StatusEffectManager.instance.ApplyEffect(i.attached, Statuses.StatusEffectType.STUN, stunDuration);
            }
		);
	}
}
