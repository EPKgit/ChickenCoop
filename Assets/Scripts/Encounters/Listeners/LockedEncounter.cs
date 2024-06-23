using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Encounters;

[System.Serializable]
public class LockedEncounter : IEncounterListener
{
    public GameObject entryDoor;
    public GameObject exitDoor;

    public LockedEncounter(Encounter encounter) : base(encounter)
    {
    }

    public override void OnTriggerEncounter()
    {
        if (entryDoor != null)
        {
            entryDoor.SetActive(true);
        }
        if (exitDoor != null)
        {
            exitDoor.SetActive(true);
        }
    }

    public override void OnEndEncounter()
    {
        if (entryDoor != null)
        {
            entryDoor.SetActive(false);
        }
        if (exitDoor != null)
        {
            exitDoor.SetActive(false);
        }
    }

    public override void OnDrawGizmosSelected(Encounter encounter)
    {
        Gizmos.color = new Color(1, 1, 0, 0.5f);
        Vector3 position;
        Vector3 size;
        if (entryDoor != null)
        {
            position = new Vector3(entryDoor.transform.position.x, entryDoor.transform.position.y, 0);
            size = new Vector3(entryDoor.transform.localScale.x, entryDoor.transform.localScale.y, 0);
            Gizmos.DrawCube(position, size);
            Gizmos.DrawLine(encounter.transform.position, position);
        }

        if (exitDoor)
        {
            position = new Vector3(exitDoor.transform.position.x, exitDoor.transform.position.y, 0);
            size = new Vector3(exitDoor.transform.localScale.x, exitDoor.transform.localScale.y, 0);
            Gizmos.DrawCube(position, size);
            Gizmos.DrawLine(encounter.transform.position, position);
        }
    }
}
