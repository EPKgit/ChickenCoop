using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Encounters;
using UnityEditor;

[RequireComponent(typeof(Collider2D))]
public class LockedArenaEncounter : Encounter
{
    public OnEncounterAreaEnter trigger;
    public LockedEncounter lockedEncounter;
    public ArenaEncounter arenaEncounter;

    public LockedArenaEncounter()
    {
        trigger = new OnEncounterAreaEnter(this);
        lockedEncounter = new LockedEncounter(this);
        arenaEncounter = new ArenaEncounter(this);
        encounterTriggers = new IEncounterTrigger[1] { trigger };
        encounterListeners = new IEncounterListener[2] { lockedEncounter, arenaEncounter };
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
    }
}
