using System.Collections;
using System.Collections.Generic;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpineRain_Ability : Ability
{
    public override uint ID => 204;

    public float damage = 20.0f;
    public float hitboxDuration = 4f;
    public float interval = 0.1f;
    public HitboxDataAsset hitboxAsset;

    //RED
    public float redRadius = 3.5f;
    public float allySpeedBoost = 0.2f;

    private LayerMask defaultMask;
    private LayerMask redMask;
    public override void Initialize(PlayerAbilities pa)
    {
        base.Initialize(pa);
        defaultMask = LayerMask.GetMask("Enemies");
        redMask = defaultMask | LayerMask.GetMask("Player");
    }

    public override void OnUpgrade(AbilityUpgradeSlot slot)
    {
        if (slot == AbilityUpgradeSlot.RED)
        {
            IncrementTargetingType();
        }
    }

    public override void OnDowngrade(AbilityUpgradeSlot slot)
    {
        if (slot == AbilityUpgradeSlot.RED)
        {
            DecrementTargetingType();
        }
    }

    protected override void UseAbility()
    {
        base.UseAbility();

        var hitboxData = HitboxData.GetBuilder(hitboxAsset)
            .StartPosition(targetingData.inputPoint)
            .Callback(HitboxCallback)
            .Duration(hitboxDuration)
            .Radius(RedUpgraded() ? redRadius : targetingData.PreviewScale.x / 2)
            .Layer(RedUpgraded() ? redMask : defaultMask)
            .RepeatCooldown(interval);
        if (RedUpgraded())
        {
            hitboxData.UpdateCallback((hitbox) => hitbox.transform.position = playerAbilities.transform.position);
        }
        HitboxManager.instance.SpawnHitbox(hitboxData.Finalize());
    }

    void HitboxCallback(Collider2D col, Hitbox hitbox)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            StatusEffectManager.instance.ApplyEffect(col.gameObject, new Haste_StatusEffect(interval, allySpeedBoost));
            return;
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
                    .Finalize()
            );
        }        
    }
}
