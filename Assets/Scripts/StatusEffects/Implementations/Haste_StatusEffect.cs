using Statuses;
using System;
using System.Collections.Generic;
using UnityEngine;
using RawGameplayTag = System.String;

public class Haste_StatusEffect : StatusEffectBase
{
    public override StatusEffectType type => StatusEffectType.SLOW;

    public override RawGameplayTag[] flags => new RawGameplayTag[]
    {
        GameplayTagFlags.MOVEMENT_EFFECTED,
    };

    public override StatusEffectStackingType stackingPolicy => StatusEffectStackingType.CUSTOM;


    private class StackableHaste
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
    private List<StackableHaste> hastes;
    private StackableHaste current;
    private StatBlockComponent statBlockComponent;
    public Haste_StatusEffect(float duration, float hastePercentage)
    {
        this.duration = duration;
        hastes = new List<StackableHaste>();
        current = new StackableHaste() { duration = duration, percent = hastePercentage };
    }

    public override bool Tick(float deltaTime)
    {
        for (int x = hastes.Count - 1; x >= 0; --x)
        {
            if (hastes[x].Tick(deltaTime))
            {
                hastes.RemoveAt(x);
            }
        }

        if (current.Tick(deltaTime))
        {
            if(hastes.Count <= 0)
            {
                return true;
            }
            else
            {
                float maxSlow = float.PositiveInfinity;
                int index = -1;
                for (int x = 0; x < hastes.Count; ++x)
                {
                    if (hastes[x].percent <= maxSlow)
                    {
                        index = x;
                        maxSlow = hastes[x].percent;
                    }
                }
                StackableHaste newCurrent = hastes[index];
                hastes.RemoveAt(index);
                SetNewCurrent(newCurrent);
            }
        }
        duration = current.duration;
        return false;
        
    }

    void SetNewCurrent(StackableHaste newCurrent)
    {
        StackableHaste oldCurrent = current;
        current = newCurrent;
        statBlockComponent.GetStat(StatName.MovementSpeed).RemoveMultiplicativeModifier(oldCurrent.handle);
        current.handle = statBlockComponent.GetStat(StatName.MovementSpeed).AddMultiplicativeModifier(1 + current.percent);
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
        Haste_StatusEffect other = challenger as Haste_StatusEffect;
        if (other == null)
        {
            throw new Exception("ERROR: slow stacking with non slow?");
        }

        // less than because slows are inverted
        if(other.current.percent > current.percent)
        {
            hastes.Add(current);
            SetNewCurrent(other.current);
        }
        else
        {
            hastes.Add(other.current);
        }
        return true;
    }
}