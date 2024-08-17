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
    public GameObject spinePrefab;
    public KnockbackPreset knockbackDefault;

    // RED
    public KnockbackPreset knockbackRed;
    public int projectileCountRed;
    public float projectileSpreadArc;

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

        KnockbackPreset knockback = RedUpgraded() ? knockbackRed : knockbackDefault;
        Vector2 direction = targetingData.inputDirectionNormalized;
        direction = Lib.DefaultDirectionCheck(direction);
        direction *= projectileSpeed;
        
        float projectileCount = 1;
        float startingDegrees = targetingData.inputRotationZ;
        float degreeInc = 0;
        if (RedUpgraded())
        {
            projectileCount = projectileCountRed;
            startingDegrees -= (projectileSpreadArc / 2);
            if(projectileCount <= 1)
            {
                throw new System.Exception();
            }
            degreeInc = projectileSpreadArc / (projectileCount - 1);
        }

        for (int i = 0; i < projectileCount; ++i)
        {
            float rot = (startingDegrees + degreeInc * i) % 360 * Mathf.Deg2Rad;
            float x = -Mathf.Sin(rot);
            float y = Mathf.Cos(rot);
            direction = new Vector2(x, y) * projectileSpeed;
            GameObject temp = PoolManager.instance.RequestObject(spinePrefab);
            temp.GetComponent<SpineProjectile_Script>().Setup
            (
                playerAbilities.transform.position,
                direction,
                playerAbilities.gameObject,
                damage,
                projectileLifetime,
                knockback
            );
        }
    }


}
