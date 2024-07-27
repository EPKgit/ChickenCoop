using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityQueue
{
    public event AbilityCastingDelegate preAbilityCastEvent = delegate { };
    public event AbilityCastingDelegate castTickEvent = delegate { };
    public event AbilityCastingDelegate postAbilityCastEvent = delegate { };
    public event AbilityCastingDelegate preAbilityActivateEvent = delegate { };
    public event AbilityCastingDelegate postAbilityActivateEvent = delegate { };

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
            STARTING_CAST_TIME,
            CASTING,
            FINISHED_CAST_TIME,
            ACTIVATED,
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
                postAbilityActivateEvent(new AbilityEventData(currentlyTicking[x]));
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
                    if (current.ability.targetingData.isInputSet)
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
                } break;
                case AbilityInputData.AbilityInputState.STARTING_CAST_TIME:
                {
                    DebugFlags.Log(DebugFlags.Flags.ABILITYQUEUE, string.Format("Ability:{0} STARTING CAST WITH INPUT {1} {2}", a.abilityName, current.ability.targetingData.inputPoint, current.ability.targetingData.inputTarget));
                    preAbilityCastEvent(eventData);
                    currentCastTimer = 0.0f;
                    a.CleanupAllTargeting(attached);
                    current.state = AbilityInputData.AbilityInputState.CASTING;
                } break;
                case AbilityInputData.AbilityInputState.CASTING:
                {
                    currentCastTimer += Time.deltaTime * playerAbilities.stats.GetValue(StatName.CastingSpeed);
                    eventData.currentCastProgress = Mathf.Clamp(currentCastTimer / a.castTime, 0, 1);
                    castTickEvent(eventData);
                    DebugFlags.Log(DebugFlags.Flags.ABILITYQUEUE, string.Format("Ability:{0} CASTING {1}/{2}", a.abilityName, currentCastTimer, a.castTime));
                    if (currentCastTimer >= a.castTime)
                    {
                        current.state = AbilityInputData.AbilityInputState.FINISHED_CAST_TIME;
                        goto case AbilityInputData.AbilityInputState.FINISHED_CAST_TIME;
                    }
                } break;
                case AbilityInputData.AbilityInputState.FINISHED_CAST_TIME:
                {
                    postAbilityCastEvent(eventData);
                    current.state = AbilityInputData.AbilityInputState.ACTIVATED;
                    goto case AbilityInputData.AbilityInputState.ACTIVATED;
                } 
                case AbilityInputData.AbilityInputState.ACTIVATED:
                { 
                    DebugFlags.Log(DebugFlags.Flags.ABILITYQUEUE, string.Format("Ability:{0} ACTIVATED CAST WITH INPUT {1} {2}", a.abilityName, current.ability.targetingData.inputPoint, current.ability.targetingData.inputTarget));
                    preAbilityActivateEvent(eventData);
                    if (a.AttemptUseAbility())
                    {
                        if (a.IsTickingAbility)
                        {
                            currentlyTicking.Add(a);
                        }
                        else
                        {
                            a.FinishAbility();
                            postAbilityActivateEvent(eventData);
                        }
                    }
                    current.state = AbilityInputData.AbilityInputState.FINISHED;
                    goto case AbilityInputData.AbilityInputState.FINISHED;
                }
                case AbilityInputData.AbilityInputState.FINISHED:
                {
                    DebugFlags.Log(DebugFlags.Flags.ABILITYQUEUE, string.Format("Ability:{0} FINISHED", current.ability.abilityName));
                    a.CleanupAllTargeting(attached);
                    a.targetingData.isInputSet = false;
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
            postAbilityActivateEvent(new AbilityEventData(currentlyTicking[x]));
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
            Targeting.RuntimeAbilityTargetingData atd = aid.ability.targetingData;
            DebugFlags.Log(DebugFlags.Flags.ABILITYQUEUE, string.Format("ABILITY:{0} RECIEVE INPUT OF {1}", ability.abilityName, targetData));
            atd.inputPoint = targetData;
            atd.relativeInputDirection = new Vector2(atd.inputPoint.x - playerAbilities.transform.position.x, atd.inputPoint.y - playerAbilities.transform.position.y);
            atd.inputRotationZ = Vector2.SignedAngle(Vector2.up, atd.relativeInputDirection);
            atd.inputRotationZ = atd.inputRotationZ < 0 ? atd.inputRotationZ + 360.0f : atd.inputRotationZ;
            atd.inputTarget = Ability.FindTargetable(targetData, ability.targetingData.Affiliation);
            atd.isInputSet = true;
            if(!atd.isInputSet)
            {
                aid.state = AbilityInputData.AbilityInputState.ACTIVATED;
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