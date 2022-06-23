using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEncounter
{
    public class Wave
    {
        
    }

    public enum SpawnType
    {
        RANDOM,
        MAX,
    }
    public class EncounterSpawnData
    {
        public EnemyType type;
        public int amount;
    }
}

[RequireComponent(typeof(Collider2D))]
public class LockedArenaEncounter : MonoBehaviour
{
    public Vector2[] spawnPoints;

    public EnemyEncounter encounterData;

    public GameObject door;

    void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.CompareTag("Player"))
        {
            TriggerEncounter();
        }
    }

    void TriggerEncounter()
    {

        Destroy(gameObject);
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
