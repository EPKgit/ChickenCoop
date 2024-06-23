using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Encounters;

public abstract class Encounter : MonoBehaviour
{
    public IEncounterTrigger[] encounterTriggers;
    public IEncounterListener[] encounterListeners;

    public void Awake()
    {
        foreach(var encounterTrigger in encounterTriggers)
        {
            encounterTrigger.encounter = this;
        }
        foreach (var encounterListener in encounterListeners)
        {
            encounterListener.encounter = this;
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        foreach(IEncounterTrigger encounterTrigger in encounterTriggers)
        {
            encounterTrigger.OnTriggerEnter2D(other);
        }
    }

    public void TriggerEncounter()
    {
        foreach(IEncounterListener encounterListener in encounterListeners)
        {
            encounterListener.OnTriggerEncounter();
        }
        EncounterManager.instance.OnEncounterStateChange += OnEncounterStateChange;
        EncounterManager.instance.StartEncounter();
    }

    void EndEncounter()
    {
        foreach (IEncounterListener encounterListener in encounterListeners)
        {
            encounterListener.OnEndEncounter();
        }
        EncounterManager.instance.OnEncounterStateChange -= OnEncounterStateChange;
        Destroy(gameObject);
    }

    public virtual void OnEncounterStateChange(EncounterState oldState, EncounterState newState)
    {
        if (newState == EncounterState.ENCOUNTER_ENDED)
        {
            EndEncounter();
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        foreach(var encounterTrigger in encounterTriggers)
        {
            encounterTrigger.OnDrawGizmosSelected(this);
        }

        foreach (var encounterListener in encounterListeners)
        {
            encounterListener.OnDrawGizmosSelected(this);
        }
    }
}
