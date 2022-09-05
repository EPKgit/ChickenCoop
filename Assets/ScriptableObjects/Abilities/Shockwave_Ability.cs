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
        Shockwave shockwave = temp.GetComponent<Shockwave>();
        shockwave.Setup
		(
			playerAbilities.transform.position,
            lifetime,
            thickness,
            aoe,
            playerAbilities.gameObject,
            (IDamagable i) =>
            {
                i.Damage
                (
                    HealthChangeData.GetBuilder()
                        .KnockbackData(KnockbackPreset.BIG)
                        .Damage(damage)
                        .LocalSource(temp)
                        .OverallSource(playerAbilities.gameObject)
                        .Target(i)
                        .LocationFunction(() => { return shockwave.GetIntersectLocation(); })
                        .Finalize()
                );
                StatusEffectManager.instance.ApplyEffect(i.attached, Statuses.StatusEffectType.STUN, stunDuration);
            }
		);
	}
}
