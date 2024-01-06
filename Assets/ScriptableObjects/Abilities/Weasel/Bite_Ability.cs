using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Bite_Ability : Ability
{
    public float damage = 1.0f;
    public float hitboxDuration = 0.25f;
    public float hitboxRadius = 0.5f;
    public KnockbackPreset knockbackPreset;

    // RED UPGRADE
    public float stunDuration = 1.0f;

    // BLUE UPGRADE
    public int multiHit = 2;

    // YELLOW UPGRADE
    public float otherAbilityCooldownTick = 1.0f;

    public override void Initialize(PlayerAbilities pa)
	{
		base.Initialize(pa);
    }

    public override void FinishAbility()
    {
        base.FinishAbility();
    }

    protected override void UseAbility()
	{
        base.UseAbility();
        targetingData.inputPoint = ClampPointWithinRange(targetingData.inputPoint, targetingData.range - 0.5f);
        HitboxManager.instance.SpawnHitbox
        (
            HitboxData.GetBuilder()
                .Duration(hitboxDuration)
                .Shape(HitboxShape.CIRCLE)
                .StartRotationZ(targetingData.inputRotationZ)
                .StartPosition(targetingData.inputPoint)
                .Callback(HitboxCallback)
                .Discriminator(HitboxManager.Discriminators.Damagables)
                .RepeatPolicy(HitboxRepeatPolicy.ONLY_ONCE)
                .Radius(hitboxRadius)
                .Finalize()
        );
    }

    void HitboxCallback(Collider2D col)
    {
        IDamagable damageInterface = col.GetComponent<IDamagable>();
        if(damageInterface == null)
        {
            return;
        }

        int hitCount = GetAbilityUpgradeStatus(AbilityUpgradeSlot.BLUE) ? multiHit : 1;
        for (int x = 0; x < hitCount; ++x)
        {
            damageInterface.Damage
            (
                HealthChangeData.GetBuilder()
                    .Damage(damage)
                    .BothSources(playerAbilities.gameObject)
                    .Target(col.gameObject)
                    //.KnockbackData(knockbackPreset)
                    .Finalize()
            );
        }

        if(GetAbilityUpgradeStatus(AbilityUpgradeSlot.RED))
        {
            StatusEffectManager.instance.ApplyEffect(damageInterface.attached, Statuses.StatusEffectType.STUN, stunDuration);
        }

        if (GetAbilityUpgradeStatus(AbilityUpgradeSlot.YELLOW))
        {
            foreach (Ability a in playerAbilities.abilities)
            {
                a.Cooldown(otherAbilityCooldownTick);
            }
        }
    }
}
