using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EveryXAbilityNoCD_Ability : Ability
{
    public override uint ID => 11;

    public float SetCDTo = 0.5f;
    public int NumAbilitiesBeforeReset = 5;

    private int counter = 0;

    public override void Initialize(PlayerAbilities pa)
	{
		base.Initialize(pa);
        pa.postAbilityActivateEvent += OnAbilityCast;
    }

    public override void Cleanup(PlayerAbilities pa)
    {
        base.Cleanup(pa);
        pa.postAbilityActivateEvent -= OnAbilityCast;
    }

    void OnAbilityCast(AbilityEventData a)
    {
        ++counter;
        if(counter >= NumAbilitiesBeforeReset)
        {
            if (a.ability.currentCooldownTimer > SetCDTo)
            {
                a.ability.Cooldown(a.ability.currentCooldownTimer - SetCDTo);
            }
            counter = 0;
        }
    }
}
