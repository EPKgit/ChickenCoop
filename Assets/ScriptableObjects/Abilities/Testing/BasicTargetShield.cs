using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicTargetShield : Ability
{
    public override uint ID => 15;

    public float amount;

    public override void Initialize(PlayerAbilities pa)
	{
		base.Initialize(pa);
	}

    protected override void UseAbility()
	{
        base.UseAbility();
        IShieldable shieldable = Lib.FindUpwardsInTree<IShieldable>(targetingData.inputTarget.Attached);
        if(shieldable != null)
        {
            shieldable.ApplyShield
            (
                ShieldApplicationData.GetBuilder()
                    .BothSources(playerAbilities.gameObject)
                    .Target(shieldable)
                    .Value(amount)
                    .Finalize()
            );
        }
	}
}
