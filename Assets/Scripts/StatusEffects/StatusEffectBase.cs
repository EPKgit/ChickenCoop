using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Statuses;

using RawGameplayTag = System.String;

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
    KNOCKBACK_IMMUNITY,
    INVULERNABILITY,
    MAX,
}
public enum StatusEffectStackingType
{
    OVERRIDE_MAX,
    ADD_DURATION,
    MULTIPLE_INSTANCES,
    CUSTOM,
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
    public abstract RawGameplayTag[] flags
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
        protected set;
    }
    public List<GameplayTagInternals.GameplayTagID> tagHandles;

    public event Action<StatusEffectBase> OnRemoved = delegate { };
    public event Action<float> OnDurationChanged = delegate { };

    /// <summary>
    /// Called to apply basic status effect processing on a target, will apply basic tags then go into per-status custom processing
    /// after determining if we have a valid target
    /// </summary>
    /// <param name="tagComponent">The target</param>
    public void OnApplication(GameplayTagComponent tagComponent) 
    {
        if (OnApplicationInternal(tagComponent))
        {
            tagHandles = new List<GameplayTagInternals.GameplayTagID>();
            foreach (RawGameplayTag flag in flags)
            {
                tagHandles.Add(tagComponent.tags.AddTag(flag));
            }
        }
    }

    /// <summary>
    /// Called to attempt to perform per-status calculations. Can fail if the gameobject applied to is not a valid target.
    /// </summary>
    /// <param name="appliedTo">The gameobject we are attempting</param>
    /// <returns>True if we have correctly applied and wish to continue, false if we have an invalid target and want to stop</returns>
    protected virtual bool OnApplicationInternal(GameplayTagComponent appliedTo) 
    {
        return true;
    }

    /// <summary>
    /// Called to cleanup the status effect, remove tags and any per-status custom processing that need to be removed
    /// </summary>
    /// <param name="tagComponent">The target</param>
    public void OnRemoval(GameplayTagComponent appliedTo) 
    { 
        foreach(GameplayTagInternals.GameplayTagID i in tagHandles)
        {
            appliedTo.tags.RemoveFirstTagWithID(i);
        }
        OnRemovalInternal(appliedTo);
        OnRemoved.Invoke(this);
    }
    protected virtual void OnRemovalInternal(GameplayTagComponent tagComponent) { }

    /// <summary>
    /// Can be implemented on a per status basis for status that need custom stacking logic. Needs to be implemented if you
    /// have a stacking type of custom
    /// </summary>
    /// <param name="challenger">The status effect that is being applied on top</param>
    /// <returns>True if we are good and don't need to continue adding the status effect, false if we are going to apply the status effect anyway</returns>
    public virtual bool HandleStacking(StatusEffectBase challenger)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Called on every frame for the status effect
    /// </summary>
    /// <returns>True if the status effect wants to terminate early</returns>
    public virtual bool Tick(float deltaTime) 
    {
        duration -= deltaTime;
        return false;
    }

    /// <summary>
    /// Change the remaining duration of a status effect, often when a new stack of status effect is applied
    /// </summary>
    /// <param name="f">The new duration</param>
    public void SetNewDuration(float f)
    {
        duration = f;
        OnDurationChanged.Invoke(f);
    }

    /// <summary>
    /// Lookup table to get an instance of a basic status effect that only has a duration
    /// </summary>
    /// <param name="type">The type of status effect to create</param>
    /// <param name="duration">The duration of the status effect</param>
    /// <returns>The status effect created</returns>
    /// <exception cref="System.Exception">
    ///     If our status effect hasn't been added to the table or it's a complex status effect that needs
    ///     more inforation than just a duration
    /// </exception>
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
            case StatusEffectType.KNOCKBACK_IMMUNITY:
                return new KnockbackImmunity_StatusEffect() { duration = duration };
            case StatusEffectType.INVULERNABILITY:
                return new Invulnerability_StatusEffect() { duration = duration };
            default:
                throw new System.Exception("ERROR: Attempt to apply status that hasn't been implemented yet " + type);
        }
    }
}
