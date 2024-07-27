using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityQueue
{
    public event AbilityCastingDelegate OnAbilityCasting = delegate { };
    public event AbilityCastingDelegate OnAbilityCastTick = delegate { };
    public event AbilityCastingDelegate OnAbilityCasted = delegate { };
    public event AbilityCastingDelegate OnAbilityActivating = delegate { };
    public event AbilityCastingDelegate OnAbilityActivated = delegate { };
    public event AbilityCastingDelegate OnAbilityCancelled = delegate { };

    private class AbilityInputData
    {
        public AbilityInputData(Ability a)
        {
            ability = a;
            state = AbilityInputState.WAITING_IN_QUEUE;
        }
        public Ability ability;
        public enum AbilityInputState
        {
            WAITING_IN_QUEUE,
            START_PREVIEW,
            WAITING_FOR_INPUT,
            RECEIVED_INPUT,
            STARTING_CAST_TIME,
            CASTING,
            FINISHED_CAST_TIME,
            ACTIVATED,
            CANCELLED,
            FINISHED,
            MAX,
        }
        public AbilityInputState state;

        public override string ToString()
        {
            return ability.ToString() + " " + state.ToString();
        }
    }
    private Queue<AbilityInputData> abilityInputQueue = new Queue<AbilityInputData>();

    private List<Ability> currentlyTicking = new List<Ability>();

    private PlayerAbilities playerAbilities;
    private float currentCastTimer = 0.0f;

    public AbilityQueue(PlayerAbilities pa)
    {
        playerAbilities = pa;
    }

    public void Update(GameObject attached)
    {
        for (int x = currentlyTicking.Count - 1; x >= 0; --x)
        {
            if (currentlyTicking[x].Tick(Time.deltaTime))
            {
                OnAbilityActivated(new AbilityEventData(currentlyTicking[x]));
                currentlyTicking[x].FinishAbility();
                currentlyTicking.RemoveAt(x);
            }
        }
        if (abilityInputQueue.Count != 0)
        {
            AbilityInputData current = abilityInputQueue.Peek();
            Ability a = current.ability;
            AbilityEventData eventData = new AbilityEventData(a);
            DebugFlags.Log(DebugFlags.Flags.ABILITYQUEUE, string.Format("ABILITY QUEUE UPDATE WITH {0}", current));

            switch (current.state)
            {
                case AbilityInputData.AbilityInputState.WAITING_IN_QUEUE:
                {
                    DebugFlags.Log(DebugFlags.Flags.ABILITYQUEUE, string.Format("Ability:{0} STARTED FRAME WAITING IN QUEUE, STARTING", current.ability.abilityName));
                    current.state = AbilityInputData.AbilityInputState.START_PREVIEW;
                    goto case AbilityInputData.AbilityInputState.START_PREVIEW;
                }

                case AbilityInputData.AbilityInputState.START_PREVIEW:
                {
                    DebugFlags.Log(DebugFlags.Flags.ABILITYQUEUE, string.Format("Ability:{0} START PREVIEW", a.abilityName));
                    current.ability.targetingData.Preview(a, attached);
                    current.state = AbilityInputData.AbilityInputState.WAITING_FOR_INPUT;
                    goto case AbilityInputData.AbilityInputState.WAITING_FOR_INPUT;
                }

                case AbilityInputData.AbilityInputState.WAITING_FOR_INPUT:
                {
                    DebugFlags.Log(DebugFlags.Flags.ABILITYQUEUE, string.Format("Ability:{0} WAITING FOR INPUT", a.abilityName));
                    current.ability.targetingData.PreviewUpdate(a, attached);
                    // we leave this state from AbilityRecieveInput which gets input externally
                } break;

                case AbilityInputData.AbilityInputState.RECEIVED_INPUT:
                {
                    if (a.castTime > 0)
                    {
                        current.state = AbilityInputData.AbilityInputState.STARTING_CAST_TIME;
                        goto case AbilityInputData.AbilityInputState.STARTING_CAST_TIME;
                    }
                    else
                    {
                        current.state = AbilityInputData.AbilityInputState.ACTIVATED;
                        goto case AbilityInputData.AbilityInputState.ACTIVATED;
                    }
                }

                case AbilityInputData.AbilityInputState.STARTING_CAST_TIME:
                {
                    DebugFlags.Log(DebugFlags.Flags.ABILITYQUEUE, string.Format("Ability:{0} STARTING CAST WITH INPUT {1} {2}", a.abilityName, current.ability.targetingData.inputPoint, current.ability.targetingData.inputTarget));
                    OnAbilityCasting(eventData);
                    currentCastTimer = 0.0f;
                    a.CleanupAllTargeting(attached);
                    current.state = AbilityInputData.AbilityInputState.CASTING;
                } break;

                case AbilityInputData.AbilityInputState.CASTING:
                {
                    currentCastTimer += Time.deltaTime * playerAbilities.stats.GetValue(StatName.CastingSpeed);
                    eventData.currentCastProgress = Mathf.Clamp(currentCastTimer / a.castTime, 0, 1);
                    OnAbilityCastTick(eventData);
                    DebugFlags.Log(DebugFlags.Flags.ABILITYQUEUE, string.Format("Ability:{0} CASTING {1}/{2}", a.abilityName, currentCastTimer, a.castTime));
                    if (currentCastTimer >= a.castTime)
                    {
                        current.state = AbilityInputData.AbilityInputState.FINISHED_CAST_TIME;
                        goto case AbilityInputData.AbilityInputState.FINISHED_CAST_TIME;
                    }
                } break;

                case AbilityInputData.AbilityInputState.FINISHED_CAST_TIME:
                {
                    OnAbilityCasted(eventData);
                    current.state = AbilityInputData.AbilityInputState.ACTIVATED;
                    goto case AbilityInputData.AbilityInputState.ACTIVATED;
                } 

                case AbilityInputData.AbilityInputState.ACTIVATED:
                { 
                    DebugFlags.Log(DebugFlags.Flags.ABILITYQUEUE, string.Format("Ability:{0} ACTIVATED CAST WITH INPUT {1} {2}", a.abilityName, current.ability.targetingData.inputPoint, current.ability.targetingData.inputTarget));
                    OnAbilityActivating(eventData);
                    if (a.AttemptUseAbility())
                    {
                        if (a.IsTickingAbility)
                        {
                            currentlyTicking.Add(a);
                        }
                        else
                        {
                            a.FinishAbility();
                            OnAbilityActivated(eventData);
                        }
                    }
                    current.state = AbilityInputData.AbilityInputState.FINISHED;
                    goto case AbilityInputData.AbilityInputState.FINISHED;
                }

                case AbilityInputData.AbilityInputState.CANCELLED:
                {
                    DebugFlags.Log(DebugFlags.Flags.ABILITYQUEUE, string.Format("Ability:{0} CANCELLED", current.ability.abilityName));
                    OnAbilityCancelled(eventData);
                    a.CleanupAllTargeting(attached);
                    a.targetingData.ResetInput();
                    abilityInputQueue.Dequeue();
                }
                break;

                case AbilityInputData.AbilityInputState.FINISHED:
                {
                    DebugFlags.Log(DebugFlags.Flags.ABILITYQUEUE, string.Format("Ability:{0} FINISHED", current.ability.abilityName));
                    a.CleanupAllTargeting(attached);
                    a.targetingData.ResetInput();
                    abilityInputQueue.Dequeue();
                } break;
            }
        }
    }

    public void AbilityStarted(Ability a)
    {
        if(a.isPassive)
        {
            currentlyTicking.Add(a);
            return;
        }
        if(!a.IsCastable() || playerAbilities.InventoryOpen())
        {
            return;
        }
        foreach(var data in abilityInputQueue)
        {
            if(data.ability == a)
            {
                DebugFlags.Log(DebugFlags.Flags.ABILITYQUEUE, string.Format("FAILED ENQUEING {0} BECAUSE IT WAS ALREADY IN QUEUE", a.abilityName));
                return;
            }
        }
        DebugFlags.Log(DebugFlags.Flags.ABILITYQUEUE, string.Format("ENQUEING {0}", a.abilityName));
        abilityInputQueue.Enqueue(new AbilityInputData(a));
    }

    public bool AbilityEndedExternal(Ability a)
    {
        for (int x = 0; x < currentlyTicking.Count; ++x)    
        {
            if (currentlyTicking[x] != a)
            {
                continue;
            }
            OnAbilityActivated(new AbilityEventData(currentlyTicking[x]));
            currentlyTicking[x].FinishAbility();
            currentlyTicking.RemoveAt(x);
            return true;
        }
        return false;
    }

    

    public void AbilityRecieveInput(Ability ability, Vector2 targetData)
    {
        AbilityInputData aid = null;
        foreach(var data in abilityInputQueue)
        {
            if (data.ability == ability)
            {
                aid = data;
                break;
            }
        }
        if(aid != null)
        {
            if (aid.ability.ValidateTargeting(targetData))
            {
                aid.state = AbilityInputData.AbilityInputState.RECEIVED_INPUT;
            }
            else
            {
                aid.state = AbilityInputData.AbilityInputState.CANCELLED;
            }

        }
    }

    public List<string> GetCurrentlyTickingAbilities()
    {
        if (currentlyTicking == null)
        {
            return null;
        }
        List<string> ret = new List<string>();
        foreach (Ability a in currentlyTicking)
        {
            ret.Add(a.ToString());
        }
        return ret;
    }
}

public class AbilityEventData
{
    public AbilityEventData(Ability a)
    {
        ability = a;
    }
    public Ability ability;
    public float currentCastProgress;
}