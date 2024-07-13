using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Scurry_Ability : Ability
{
    public override uint ID => 101;

    public GameObject scurryVFXPrefab;
    public float movementSpeedMultiplier = 1.5f;
    public float damageMultiplier = 2.0f;
    public float smokeCloudDuration = 3.0f;

    private uint? movementSpeedBonusHandle;
    private uint? dmgStatusHandle;
    private StatBlock statBlock;
    public override void Initialize(PlayerAbilities pa)
	{
		base.Initialize(pa);
        statBlock = pa.stats;
        pa.postAbilityCastEvent += OnPostAbilityCast;
        scurryVFXPrefab.GetComponent<VFXPoolable>().fixedDuration = smokeCloudDuration;
    }
    
    public override void Cleanup(PlayerAbilities pa)
    {
        base.Cleanup(pa);
        pa.postAbilityCastEvent -= OnPostAbilityCast;
    }

    protected override void UseAbility()
    {
        base.UseAbility();
        movementSpeedBonusHandle = statBlock.GetStat(StatName.MovementSpeed)?.AddMultiplicativeModifier(movementSpeedMultiplier);
        dmgStatusHandle = statBlock.GetStat(StatName.DamagePercentage)?.AddMultiplicativeModifier(damageMultiplier);

        if(GetAbilityUpgradeStatus(AbilityUpgradeSlot.RED))
        {
            PoolManager.instance.RequestObject(scurryVFXPrefab).transform.position = playerAbilities.transform.position;
            HitboxManager.instance.SpawnHitbox
            (
                HitboxData.GetBuilder()
                    .Duration(smokeCloudDuration)
                    .Delay(0.5f)
                    .ShapeType(HitboxShapeType.CIRCLE)
                    .StartPosition(playerAbilities.transform.position)
                    .Callback(HitboxCallback)
                    .Discriminator(HitboxManager.Discriminators.Damagables)
                    .RepeatPolicy(HitboxRepeatPolicy.COOLDOWN)
                    .RepeatCooldown(0.1f)
                    .Radius(2)
                    .Finalize()
            );
        }
    }

    void HitboxCallback(Collider2D col, Hitbox hitbox)
    {
        IDamagable damageInterface = col.GetComponent<IDamagable>();
        if (damageInterface == null)
        {
            return;
        }
        StatusEffectManager.instance.ApplyEffect(damageInterface.attached, Statuses.StatusEffectType.STUN, 0.5f);
        damageInterface.Damage
        (
            HealthChangeData.GetBuilder()
                .Damage(1)
                .BothSources(playerAbilities.gameObject)
                .Target(col.gameObject)
                //.KnockbackData(knockbackPreset)
                .Finalize()
        );
    }

    void OnPostAbilityCast(AbilityEventData aed)
    {
        if(aed.ability.abilityTags.Contains(GameplayTagFlags.ABILITY_DAMAGE))
        {
            playerAbilities.AbilityEndedExternal(this);
        }
    }

    public override void FinishAbility()
    {
        base.FinishAbility();
        if(movementSpeedBonusHandle.HasValue)
        {
            statBlock.GetStat(StatName.MovementSpeed)?.RemoveMultiplicativeModifier(movementSpeedBonusHandle.Value);
            movementSpeedBonusHandle = null;
        }
        if (dmgStatusHandle.HasValue)
        {
            statBlock.GetStat(StatName.DamagePercentage)?.RemoveMultiplicativeModifier(dmgStatusHandle.Value);
            dmgStatusHandle = null;
        }
    }
}
