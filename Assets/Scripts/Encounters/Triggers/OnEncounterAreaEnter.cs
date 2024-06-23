using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnEncounterAreaEnter : IEncounterTrigger
{
    public OnEncounterAreaEnter(Encounter encounter) : base(encounter)
    {
    }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            encounter.TriggerEncounter();
        }
    }
}
