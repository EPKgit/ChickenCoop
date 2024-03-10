using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicTargetHeal : Ability
{
    public override uint ID => 4;

    public float amount;

    public override void Initialize(PlayerAbilities pa)
	{
		base.Initialize(pa);
	}

    protected override void UseAbility()
	{
        base.UseAbility();
        IHealable healable = Lib.FindUpwardsInTree<IHealable>(targetingData.inputTarget.Attached);
        if(healable != null)
        {
            healable.Heal
            (
                HealthChangeData.GetBuilder()
                    .Healing(amount)
                    .BothSources(playerAbilities.gameObject)
                    .Target(healable)
                    .Finalize()
            );
        }
	}
}
