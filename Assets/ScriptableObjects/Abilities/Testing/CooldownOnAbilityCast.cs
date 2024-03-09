using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CooldownOnAbilityCast : Ability
{
    public float amount = 0.5f;
    
    public override void Initialize(PlayerAbilities pa)
    {
        base.Initialize(pa);
        pa.postAbilityCastEvent += OnAbilityCast;
    }

    public override void Cleanup(PlayerAbilities pa)
    {
        base.Cleanup(pa);
        pa.postAbilityCastEvent -= OnAbilityCast;
    }

    public void OnAbilityCast(AbilityEventData aed)
    {
        foreach(Ability a in playerAbilities.abilities)
        {
            a.Cooldown(amount);
        }
    }
}
