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

    public override void Initialize(PlayerAbilities pa)
    {
        base.Initialize(pa);
    }

    public override void OnUpgrade(AbilityUpgradeSlot slot)
    {
        //if (slot == AbilityUpgradeSlot.BLUE)
        //{
            //IncrementTargetingType();
        //}
    }

    public override void OnDowngrade(AbilityUpgradeSlot slot)
    {
        //if (slot == AbilityUpgradeSlot.BLUE)
        //{
            //DecrementTargetingType();
        //}
    }

    protected override void UseAbility()
    {
        base.UseAbility();
        targetingData.inputPoint = ClampPointWithinRange(targetingData.inputPoint, targetingData.Range);

        var hitboxData = HitboxData.GetBuilder(hitboxAsset)
            .StartPosition(targetingData.inputPoint)
            .Callback(HitboxCallback)
            .Duration(hitboxDuration)
            .RepeatCooldown(interval)
            .Finalize();
        HitboxManager.instance.SpawnHitbox(hitboxData);
    }

    void HitboxCallback(Collider2D col, Hitbox hitbox)
    {
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
