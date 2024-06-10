using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipFlopCooldown_Ability : Ability
{
    public override uint ID => 12;

    public float increaseMultiplier;
    public float decreaseMultiplier;
    public float interval;

    private bool increasing;
    private float timer;

    public override void Initialize(PlayerAbilities pa)
    {
        base.Initialize(pa);
        foreach(Ability a in pa.Abilities)
        {
            a.preCooldownTick += OnCooldownTick;
        }
        pa.abilityChanged += OnAbilityChanged;
        increasing = false;
        timer = interval;
    }

    public override void Cleanup(PlayerAbilities pa)
    {
        base.Cleanup(pa);
        pa.abilityChanged -= OnAbilityChanged;
        foreach (Ability a in pa.Abilities)
        {
            a.preCooldownTick -= OnCooldownTick;
        }
    }

    void OnAbilityChanged(Ability previousAbility, Ability newAbility, AbilitySlot slot, PlayerAbilities.AbilityChangeType type)
    {
        if(previousAbility != null)
        {
            previousAbility.preCooldownTick -= OnCooldownTick;
        }
        if (newAbility != null)
        {
            newAbility.preCooldownTick -= OnCooldownTick;
            newAbility.preCooldownTick += OnCooldownTick;
        }
    }

    public override bool Tick(float deltaTime)
    {
        timer -= deltaTime;
        if(timer <= 0)
        {
            increasing = !increasing;
            timer = interval;
        }
        return base.Tick(deltaTime);
    }

    void OnCooldownTick(MutableCooldownTickData data)
    {
        data.tickDelta.AddMultiplicativeModifier(increasing ? increaseMultiplier : decreaseMultiplier);
    }
}
