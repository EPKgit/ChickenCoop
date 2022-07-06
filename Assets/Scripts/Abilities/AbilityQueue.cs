using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityQueue
{
    public event AbilityCastingDelegate preAbilityCastEvent = delegate { };
    public event AbilityCastingDelegate postAbilityCastEvent = delegate { };

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
            CASTING,
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
                postAbilityCastEvent(new AbilityEventData(currentlyTicking[x]));
                currentlyTicking[x].FinishAbility();
                currentlyTicking.RemoveAt(x);
            }
        }
        if (abilityInputQueue.Count != 0)
        {
            AbilityInputData current = abilityInputQueue.Peek();
            Ability a = current.ability;
            //DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.ABILITYQUEUE, string.Format("ABILITY QUEUE UPDATE WITH {0}", current));

            switch (current.state)
            {
                case AbilityInputData.AbilityInputState.WAITING_IN_QUEUE:
                {
                    DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.ABILITYQUEUE, string.Format("Ability:{0} STARTED FRAME WAITING IN QUEUE, STARTING", current.ability.name));
                    current.state = AbilityInputData.AbilityInputState.START_PREVIEW;
                    goto case AbilityInputData.AbilityInputState.START_PREVIEW;
                }
                case AbilityInputData.AbilityInputState.START_PREVIEW:
                {
                    DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.ABILITYQUEUE, string.Format("Ability:{0} START PREVIEW", a.name));
                    current.ability.targetingData.Preview(a, attached);
                    current.state = AbilityInputData.AbilityInputState.WAITING_FOR_INPUT;
                    goto case AbilityInputData.AbilityInputState.WAITING_FOR_INPUT;
                }
                case AbilityInputData.AbilityInputState.WAITING_FOR_INPUT:
                {
                    //DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.ABILITYQUEUE, string.Format("Ability:{0} WAITING FOR INPUT", a.name));
                    current.ability.targetingData.PreviewUpdate(a, attached);
                    if (current.ability.targetingData.isInputSet)
                    {
                        current.state = AbilityInputData.AbilityInputState.CASTING;
                        goto case AbilityInputData.AbilityInputState.CASTING;
                    }
                } break;
                case AbilityInputData.AbilityInputState.CASTING:
                {
                    DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.ABILITYQUEUE, string.Format("Ability:{0} STARTING CAST WITH INPUT {1} {2}", a.name, current.ability.targetingData.inputPoint, current.ability.targetingData.inputTarget));
                    preAbilityCastEvent(new AbilityEventData(a));
                    if (a.AttemptUseAbility())
                    {
                        if (a.tickingAbility)
                        {
                            currentlyTicking.Add(a);
                        }
                        else
                        {
                            postAbilityCastEvent(new AbilityEventData(a));
                            a.FinishAbility();
                        }
                    }
                    current.state = AbilityInputData.AbilityInputState.FINISHED;
                    goto case AbilityInputData.AbilityInputState.FINISHED;
                }
                case AbilityInputData.AbilityInputState.FINISHED:
                {
                    DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.ABILITYQUEUE, string.Format("Ability:{0} FINISHED", current.ability.name));
                    a.targetingData.isInputSet = false;
                    a.CleanupAllTargeting(attached);
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
                DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.ABILITYQUEUE, string.Format("FAILED ENQUEING {0} BECAUSE IT WAS ALREADY IN QUEUE", a.name));
                return;
            }
        }
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.ABILITYQUEUE, string.Format("ENQUEING {0}", a.name));
        abilityInputQueue.Enqueue(new AbilityInputData(a));
    }

    public void AbilityRecieveInput(Ability a, Vector2 targetData)
    {
        AbilityInputData aid = null;
        foreach(var data in abilityInputQueue)
        {
            if (data.ability == a)
            {
                aid = data;
                break;
            }
        }
        if(aid != null)
        {
            Targeting.AbilityTargetingData atd = aid.ability.targetingData;
            DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.ABILITYQUEUE, string.Format("ABILITY:{0} RECIEVE INPUT OF {1}", a.name, targetData));
            atd.inputPoint = targetData;
            atd.inputTarget = Ability.FindTargetable(targetData, a.targetingData.affiliation);
            atd.isInputSet = true;
            if(!atd.isInputSet)
            {
                aid.state = AbilityInputData.AbilityInputState.FINISHED;
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
}