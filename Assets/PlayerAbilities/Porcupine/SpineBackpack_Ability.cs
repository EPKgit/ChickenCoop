using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpineBackpack_Ability : Ability
{
    public override uint ID => 202;

    public GameObject backpackPrefab;

    public float damage = 1.0f;
    public float spineCount = 1.0f;
    public float backpackLifetime = 1.5f;
    
    private float projectileSpeed = 10.0f;
    private float projectileLifetime = 0.5f;

    // BLUE UPGRADE
    private float launchTime = 0.5f;
    private float arcSteepness = 2.0f;


    public override void Initialize(PlayerAbilities pa)
	{
		base.Initialize(pa);
        pa.stats.RegisterStatChangeCallback(StatName.SpineDuration, OnSpineDurationChange);
        pa.stats.RegisterStatChangeCallback(StatName.SpineSpeed, OnSpineSpeedChange);
    }

    public void OnSpineDurationChange(float f)
    {
        projectileLifetime = f;
    }

    public void OnSpineSpeedChange(float f)
    {
        projectileSpeed = f;
    }

    public override void OnUpgrade(AbilityUpgradeSlot slot)
    {
        if(slot == AbilityUpgradeSlot.BLUE)
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
        GameObject temp = PoolManager.instance.RequestObject(backpackPrefab);
        SpineBackpack_Script backpack = temp.GetComponent<SpineBackpack_Script>();
        backpack.Setup
        (
            playerAbilities.transform.position,
            playerAbilities.gameObject,
            damage,
            spineCount,
            backpackLifetime,
            projectileLifetime,
            projectileSpeed,
            GetAbilityUpgradeStatus(AbilityUpgradeSlot.YELLOW)
        );

        if(GetAbilityUpgradeStatus(AbilityUpgradeSlot.BLUE))
        {
            targetingData.inputPoint = ClampPointWithinRange(targetingData.inputPoint);
            backpack.SetupLaunch(targetingData.inputPoint, launchTime, arcSteepness);
        }
        else
        {
            backpack.SetupGrounded();
        }
    }
}
