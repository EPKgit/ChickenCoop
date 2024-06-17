using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Encounters;

[RequireComponent(typeof(Collider2D))]
public abstract class LockedEncounter : MonoBehaviour
{
    public GameObject entryDoor;
    public GameObject exitDoor;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            TriggerEncounter();
        }
    }

    void TriggerEncounter()
    {
        if (entryDoor != null)
        {
            entryDoor.SetActive(true);
        }
        if (exitDoor != null)
        {
            exitDoor.SetActive(true);
        }
        EncounterManager.instance.OnEncounterStateChange += OnEncounterStateChange;
        EncounterManager.instance.StartEncounter();
        StartEncounter();
    }

    protected abstract void StartEncounter();

    void EndEncounter()
    {
        if (entryDoor != null)
        {
            entryDoor.SetActive(false);
        }
        if (exitDoor != null)
        {
            exitDoor.SetActive(false);
        }
        EncounterManager.instance.OnEncounterStateChange -= OnEncounterStateChange;
        Destroy(gameObject);
    }

    protected abstract void Cleanup();

    public virtual void OnEncounterStateChange(EncounterState oldState, EncounterState newState)
    {
        if (newState == EncounterState.ENCOUNTER_ENDED)
        {
            EndEncounter();
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 1, 1, 0.5f);
        Vector3 position = new Vector3(entryDoor.transform.position.x, entryDoor.transform.position.y, 0);
        Vector3 size = new Vector3(entryDoor.transform.localScale.x, entryDoor.transform.localScale.y, 0);
        Gizmos.DrawCube(position, size);
        Gizmos.DrawLine(transform.position, position);

        position = new Vector3(exitDoor.transform.position.x, exitDoor.transform.position.y, 0);
        size = new Vector3(exitDoor.transform.localScale.x, exitDoor.transform.localScale.y, 0);
        Gizmos.DrawCube(position, size);
        Gizmos.DrawLine(transform.position, position);
    }
}
