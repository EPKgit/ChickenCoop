using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicTargetHeal : Ability
{
    public float heal;

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
            healable.Heal(heal, playerAbilities.gameObject, playerAbilities.gameObject);
        }
	}
}
