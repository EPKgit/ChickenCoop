using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CooldownOnAbilityCast : Ability
{
    public override uint ID => 5;

    public float amount = 0.5f;
    
    public override void Initialize(PlayerAbilities pa)
    {
        base.Initialize(pa);
        pa.OnAbilityActivated += OnAbilityCast;
    }

    public override void Cleanup(PlayerAbilities pa)
    {
        base.Cleanup(pa);
        pa.OnAbilityActivated -= OnAbilityCast;
    }

    public void OnAbilityCast(AbilityEventData aed)
    {
        foreach(Ability a in playerAbilities.Abilities)
        {
            a.Cooldown(amount);
        }
    }
}
