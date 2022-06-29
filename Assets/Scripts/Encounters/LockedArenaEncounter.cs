using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LockedArenaEncounter : MonoBehaviour
{
    public Vector2[] spawnPoints;

    [SerializeField]
    public EncounterData encounterData;

    public GameObject entryDoor;
    public GameObject exitDoor;

    void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.CompareTag("Player"))
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
        EncounterManager.instance.StartEncounter(encounterData, new List<Vector2>(spawnPoints), transform.position);
    }

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
    public void OnEncounterStateChange(EncounterState oldState, EncounterState newState)
    {
        if(newState == EncounterState.ENCOUNTER_ENDED)
        {
            EndEncounter();
        }
    }

    void OnDrawGizmosSelected()
    {
        if(spawnPoints == null)
        {
            return;
        }
        Gizmos.color = Color.red;
        foreach(Vector2 v in spawnPoints)
        {
            Vector2 relativePos = v + new Vector2(transform.position.x, transform.position.y);
            Gizmos.DrawLine(transform.position, relativePos);
            Gizmos.DrawIcon(relativePos, "spawnpointgizmo.png", true);
        }
    }
}
