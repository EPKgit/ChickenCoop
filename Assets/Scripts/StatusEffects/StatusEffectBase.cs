using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Statuses;

namespace Statuses
{
public enum StatusEffectType
{
    STUN,
    SILENCE,
    ROOT,
    BLIND,
    SLOW,
    FEAR,
    // KNOCKBACK,
    MAX,
}
public enum StatusEffectStackingType
{
    OVERRIDE_MAX,
    ADD_DURATION,
    MULTIPLE_INSTANCES,
    NONE,
    MAX,
}
}

public abstract class StatusEffectBase
{
    public abstract StatusEffectType type
    {
        get;
    }
    public abstract GameplayTagFlags[] flags
    {
        get;
    }
    public virtual StatusEffectStackingType stackingPolicy
    {
        get => StatusEffectStackingType.OVERRIDE_MAX;
    }

    public float duration
    {
        get;
        private set;
    }
    public List<uint> tagHandles;

    public event Action<StatusEffectBase> OnRemoved = delegate { };
    public event Action<float> OnDurationChanged = delegate { };
    public void OnApplicationExternal(GameplayTagComponent tagComponent) 
    {
        tagHandles = new List<uint>();
        foreach (GameplayTagFlags flag in flags)
        {
            tagHandles.Add(tagComponent.tags.AddTag(flag));
        }
        OnApplication(tagComponent.gameObject);
    }
    public virtual void OnApplication(GameObject appliedTo) { }
    public void OnRemovalExternal(GameplayTagComponent tagComponent) 
    { 
        foreach(uint i in tagHandles)
        {
            tagComponent.tags.RemoveTagWithID(i);
        }
        OnRemoved.Invoke(this);
    }
    public virtual void OnRemoval() { }

    /// <summary>
    /// Called on every frame for the status effect
    /// </summary>
    /// <returns>True if the status effect wants to terminate early</returns>
    public virtual bool Tick(float deltaTime) 
    {
        duration -= deltaTime;
        return false;
    }

    public void SetNewDuration(float f)
    {
        duration = f;
        OnDurationChanged.Invoke(f);
    }

    public static StatusEffectBase GetStatusObjectByType(StatusEffectType type, float duration)
    {
        switch (type)
        {
            case StatusEffectType.STUN:
                return new Stun_StatusEffect() { duration = duration };
            case StatusEffectType.ROOT:
                return new Root_StatusEffect(){ duration = duration };
            case StatusEffectType.SILENCE:
                return new Silence_StatusEffect(){ duration = duration };
            case StatusEffectType.BLIND:
                return new Blind_StatusEffect(){ duration = duration };
            // case StatusEffectType.KNOCKBACK:
            //     return new Knockback_StatusEffect() { duration = duration };
            default:
                Debug.LogError("ERROR: Attempt to apply status that hasn't been implemented yet " + type);
                throw new System.Exception();
        }
    }
}
