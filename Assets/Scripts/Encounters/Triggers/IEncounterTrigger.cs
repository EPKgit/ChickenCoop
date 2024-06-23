using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Encounters;

public abstract class IEncounterTrigger
{
    [HideInInspector]
    public Encounter encounter;

    public IEncounterTrigger(Encounter encounter)
    {
        this.encounter = encounter;
    }

    public virtual void OnTriggerEnter2D(Collider2D other)
    {
        
    }

    public virtual void OnDrawGizmosSelected(Encounter encounter)
    {

    }
}
