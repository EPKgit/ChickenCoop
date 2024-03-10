using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PiercingSpine_Ability : Ability
{
    public override uint ID => 201;

    public float damage = 1.0f;
    public float projectileSpeed = 10.0f;
    public float projectileLifetime = 0.5f;
    public KnockbackPreset knockbackPreset;
    public GameObject spinePrefab;

    public override void Initialize(PlayerAbilities pa)
	{
		base.Initialize(pa);
        pa.stats.RegisterStatChangeCallback(StatName.SpineDuration, OnSpineDurationChange);
        pa.stats.RegisterStatChangeCallback(StatName.SpineSpeed, OnSpineSpeedChange);
    }

    public void OnSpineDurationChange(float f)
    {
        projectileLifetime = f;
        targetingData.SetRangeOverride(projectileLifetime * projectileSpeed);
    }

    public void OnSpineSpeedChange(float f)
    {
        projectileSpeed = f;
        targetingData.SetRangeOverride(projectileLifetime * projectileSpeed);
    }

    public override void OnAbilityDataUpdated()
    {
        base.OnAbilityDataUpdated();
        targetingData.SetRangeOverride(projectileLifetime * projectileSpeed);
    }

    protected override void UseAbility()
	{
        base.UseAbility();
        Vector2 direction = GetNormalizedDirectionTowardsTarget(targetingData.inputPoint);
        direction = Lib.DefaultDirectionCheck(direction);
        direction *= projectileSpeed;
        GameObject temp = PoolManager.instance.RequestObject(spinePrefab);
        temp.GetComponent<SpineProjectile_Script>().Setup
        (
            playerAbilities.transform.position,
            direction,
            playerAbilities.gameObject,
            damage,
            projectileLifetime
        );
    }


}
