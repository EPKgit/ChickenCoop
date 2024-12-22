using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Statuses;

public class StatusEffectContainerComponent : MonoBehaviour
{
    private List<StatusEffectBase> statuses;

    private GameplayTagComponent tagComponent;

    void Awake()
    {
        tagComponent = GetComponent<GameplayTagComponent>();
        statuses = new List<StatusEffectBase>();
    }

    void Update()
    {
        for (int x = statuses.Count - 1; x >= 0; --x)
        {
            if (statuses[x].duration <= 0 || statuses[x].Tick(Time.deltaTime))
            {
                statuses[x].OnRemoval(tagComponent);
                statuses.RemoveAt(x);
            }
        }
    }

    public StatusEffectBase AddStatus(StatusEffectType type, float duration)
    {
        bool b;
        return AddStatus(type, duration, out b);
    }

    public StatusEffectBase AddStatus(StatusEffectType type, float duration, out bool newStatusCreated)
    {
        return AddStatus(StatusEffectBase.GetStatusObjectByType(type, duration), out newStatusCreated);
    }

    public StatusEffectBase AddStatus(StatusEffectBase status)
    {
        bool b;
        return AddStatus(status, out b);
    }

    public StatusEffectBase AddStatus(StatusEffectBase status, out bool newStatusCreated)
    {
        foreach(StatusEffectBase s in statuses)
        {
            if(s.type == status.type)
            {
                if(HandleStacking(s, status))
                {
                    newStatusCreated = false;
                    return s;
                }
            }
        }
        if(!status.OnApplication(tagComponent))
        {
            newStatusCreated = false;
            return null;
        }
        statuses.Add(status);
        newStatusCreated = true;
        return status;
    }

    /// <summary>
    /// Handles the case when we try to apply a status effect that is already applied to the character
    /// </summary>
    /// <param name="original">The status that was already there</param>
    /// <param name="challenger">The new status that is trying to be applied</param>
    /// <returns>True if we've resolved the new status and don't want to continue. False otherwise</returns>
    bool HandleStacking(StatusEffectBase original, StatusEffectBase challenger)
    {
        switch(original.stackingPolicy)
        {
            case StatusEffectStackingType.OVERRIDE_MAX:
                original.SetNewDuration(Mathf.Max(challenger.duration, original.duration));
                return true;
            case StatusEffectStackingType.ADD_DURATION:
                original.SetNewDuration(challenger.duration + original.duration);
                return true;
            case StatusEffectStackingType.MULTIPLE_INSTANCES:
                return false;
            case StatusEffectStackingType.CUSTOM:
                return original.HandleStacking(challenger);
            default:
                throw new System.NotImplementedException();
        }
    }
}
