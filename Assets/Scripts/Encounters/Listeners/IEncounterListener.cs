using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Encounters;

public abstract class IEncounterListener
{
    [HideInInspector]
    public Encounter encounter;

    public IEncounterListener(Encounter encounter)
    {
        this.encounter = encounter;
    }

    public virtual void OnTriggerEncounter()
    {

    }

    public virtual void OnEndEncounter()
    {

    }

    public virtual void OnDrawGizmosSelected(Encounter encounter)
    {

    }
}