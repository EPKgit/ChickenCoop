using Statuses;
using System;
using System.Collections.Generic;
using UnityEngine;
using RawGameplayTag = System.String;

public class Slow_StatusEffect : StatusEffectBase
{
    public override StatusEffectType type => StatusEffectType.SLOW;

    public override RawGameplayTag[] flags => new RawGameplayTag[]
    {
        GameplayTagFlags.MOVEMENT_EFFECTED,
    };

    public override StatusEffectStackingType stackingPolicy => StatusEffectStackingType.CUSTOM;


    private class StackableSlow
    {
        public float duration;
        public float percent;
        public StatModificationHandle handle;
        public bool Tick(float dt)
        {
            duration -= dt;
            return duration <= 0;
        }
    }
    private List<StackableSlow> slows;
    private StackableSlow current;
    private StatBlockComponent statBlockComponent;
    public Slow_StatusEffect(float duration, float slowPercentage)
    {
        this.duration = duration;
        slows = new List<StackableSlow>();
        current = new StackableSlow() { duration = duration, percent = slowPercentage };
    }

    public override bool Tick(float deltaTime)
    {
        for (int x = slows.Count - 1; x >= 0; --x)
        {
            if (slows[x].Tick(deltaTime))
            {
                slows.RemoveAt(x);
            }
        }

        if (current.Tick(deltaTime))
        {
            if(slows.Count <= 0)
            {
                return true;
            }
            else
            {
                float maxSlow = float.PositiveInfinity;
                int index = -1;
                for (int x = 0; x < slows.Count; ++x)
                {
                    if (slows[x].percent <= maxSlow)
                    {
                        index = x;
                        maxSlow = slows[x].percent;
                    }
                }
                StackableSlow newCurrent = slows[index];
                slows.RemoveAt(index);
                SetNewCurrent(newCurrent);
            }
        }
        duration = current.duration;
        return false;
        
    }

    void SetNewCurrent(StackableSlow newCurrent)
    {
        StackableSlow oldCurrent = current;
        current = newCurrent;
        statBlockComponent.GetStat(StatName.MovementSpeed).RemoveMultiplicativeModifier(oldCurrent.handle);
        current.handle = statBlockComponent.GetStat(StatName.MovementSpeed).AddMultiplicativeModifier(1 - current.percent);
        SetNewDuration(current.duration);
    }

    protected override bool OnApplicationInternal(GameplayTagComponent appliedTo)
    {
        base.OnApplicationInternal(appliedTo);

        BaseMovement movement = Lib.FindDownThenUpwardsInTree<BaseMovement>(appliedTo.gameObject);
        if(movement == null)
        {
            return false;
        }

        statBlockComponent = Lib.FindDownThenUpwardsInTree<StatBlockComponent>(appliedTo.gameObject);
        if(statBlockComponent == null)
        {
            return false;
        }

        SetNewCurrent(current);
        return true;
    }

    protected override void OnRemovalInternal(GameplayTagComponent appliedTo)
    {
        base.OnRemovalInternal(appliedTo);
        statBlockComponent.GetStat(StatName.MovementSpeed).RemoveMultiplicativeModifier(current.handle);
    }

    public override bool HandleStacking(StatusEffectBase challenger)
    {
        Slow_StatusEffect other = challenger as Slow_StatusEffect;
        if (other == null)
        {
            throw new Exception("ERROR: slow stacking with non slow?");
        }

        // less than because slows are inverted
        if(other.current.percent > current.percent)
        {
            slows.Add(current);
            SetNewCurrent(other.current);
        }
        else
        {
            slows.Add(other.current);
        }
        return true;
    }
}