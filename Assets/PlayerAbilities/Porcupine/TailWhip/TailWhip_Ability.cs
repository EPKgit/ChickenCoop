using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TailWhip_Ability : Ability
{
    public override uint ID => 203;

    public float damage = 20.0f;
    public float hitboxDuration = 0.25f;
    public HitboxDataAsset hitboxAsset;

    // RED
    public float backpackKnockbackDuration = 1.0f;
    public float backpackKnockbackForce = 1.0f;

    // BLUE
    public float slowDuration = 2.0f;
    public float slowPercentage = 0.5f;
    public HitboxDataAsset blueHitboxAsset;

    private LayerMask layerMaskDefault;
    private LayerMask layerMaskRed;

    public override void Initialize(PlayerAbilities pa)
    {
        base.Initialize(pa);
        layerMaskDefault = LayerMask.GetMask("Enemies");
        layerMaskRed = layerMaskDefault | LayerMask.GetMask("PlayerDrops");
    }

    public override void OnUpgrade(AbilityUpgradeSlot slot)
    {
        if (slot == AbilityUpgradeSlot.BLUE)
        {
            IncrementTargetingType();
        }
    }

    public override void OnDowngrade(AbilityUpgradeSlot slot)
    {
        if (slot == AbilityUpgradeSlot.BLUE)
        {
            DecrementTargetingType();
        }
    }

    protected override void UseAbility()
    {
        base.UseAbility();

        Vector2 startPosition = targetingData.inputPoint + -(targetingData.inputDirectionNormalized * 1f);
        var hitboxToUse = BlueUpgraded() ? blueHitboxAsset : hitboxAsset;
        var layerMask = RedUpgraded() ? layerMaskRed : layerMaskDefault;
        var hitboxData = HitboxData.GetBuilder(hitboxToUse)
            .Layer(layerMask)
            .StartPosition(startPosition)
            .RotationInfo(targetingData.inputRotationZ, targetingData.inputDirectionNormalized)
            .Callback(HitboxCallback)
            .Duration(hitboxDuration)
            .Finalize();
        HitboxManager.instance.SpawnHitbox(hitboxData);
    }

    void HitboxCallback(Collider2D col, Hitbox hitbox)
    {
        if(RedUpgraded())
        {
            SpineBackpack_Script backpack = col.GetComponent<SpineBackpack_Script>();
            if(backpack != null) 
            {
                KnockbackData data = new KnockbackData();
                Vector2 direction = Vector2.zero;
                direction.x = -Mathf.Sin(Mathf.Deg2Rad * hitbox.data.StartRotationZ);
                direction.y = Mathf.Cos(Mathf.Deg2Rad * hitbox.data.StartRotationZ);
                data.direction = direction.normalized;
                data.duration = backpackKnockbackDuration;
                data.force = backpackKnockbackForce;
                backpack.ApplyKnockback(data);
                return;
            }
        }
        IDamagable damagable = col.GetComponent<IDamagable>();
        if (damagable != null)
        {
            damagable.Damage
            (
                HealthChangeData.GetBuilder()
                    .Damage(damage)
                    .BothSources(playerAbilities.gameObject)
                    .Target(col.gameObject)
                    .KnockbackData(KnockbackPreset.BIG)
                    .Finalize()
            );
        }
        if(BlueUpgraded())
        {
            StatusEffectManager.instance.ApplyEffect(col.gameObject, new Slow_StatusEffect(slowDuration, slowPercentage));
        }
    }
}
