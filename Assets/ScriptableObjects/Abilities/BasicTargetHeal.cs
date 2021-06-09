using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicTargetHeal : Ability, IHealingAbility
{
    public float amount
    {
        get;
        set;
    }

    public override string GetTooltip()
    {
        return string.Format(tooltipDescription, amount);
    }

    public override void Initialize(PlayerAbilities pa)
	{
		base.Initialize(pa);
	}

    protected override void UseAbility()
	{
        base.UseAbility();
        IHealable healable = Lib.FindInHierarchy<IHealable>(targetingData.inputTarget.Attached);
        if(healable != null)
        {
            healable.Heal(amount, playerAbilities.gameObject, playerAbilities.gameObject);
        }
	}
}
