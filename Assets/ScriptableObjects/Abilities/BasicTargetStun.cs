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
        // StatusEffectManager.instance.ApplyEffect(playerAbilities.gameObject, StatusEffectType.STUN, stunDuration);
        StatusEffectManager.instance.ApplyEffect(targetingData.inputTarget.Attached, StatusEffectType.STUN, stunDuration);
        StatusEffectManager.instance.ApplyEffect(targetingData.inputTarget.Attached, StatusEffectType.ROOT, stunDuration);
        StatusEffectManager.instance.ApplyEffect(targetingData.inputTarget.Attached, StatusEffectType.BLIND, stunDuration);
        StatusEffectManager.instance.ApplyEffect(targetingData.inputTarget.Attached, StatusEffectType.SILENCE, stunDuration);
        // StatusEffectManager.instance.ApplyEffect(targetingData.inputTarget.Attached, StatusEffectType.FEAR, stunDuration / 2.0f);
        stunDuration += 1.0f;
    }
}
