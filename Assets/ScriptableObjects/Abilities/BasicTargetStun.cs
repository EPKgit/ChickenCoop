using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicTargetStun : Ability
{
    public float stunDuration = 2.0f;

    public override string GetTooltip()
    {
        return string.Format(tooltipDescription, stunDuration);
    }

    public override void Initialize(PlayerAbilities pa)
	{
		base.Initialize(pa);
	}

    protected override void UseAbility()
	{
        base.UseAbility();
        StatusEffectManager.instance.ApplyEffect(targetingData.inputTarget.Attached, StatusEffectType.STUN, stunDuration);
        stunDuration += 1.0f;
    }
}
