using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Encounters;

[RequireComponent(typeof(Collider2D))]
public class LockedArenaEncounter : LockedEncounter
{
    public Vector2[] spawnPoints;
    public WaveData[] waveData;

    private int waveIndex = 0;
    private List<GameObject> spawnedEnemies;

    protected override void StartEncounter()
    {
        spawnedEnemies = new List<GameObject>();
        EncounterManager.instance.OnEncounterUpdate += WaveUpdate;
        WaveStart();
    }

    protected override void Cleanup()
    {
        foreach(GameObject enemy in spawnedEnemies)
        {
            Destroy(enemy);
        }
        EncounterManager.instance.OnEncounterUpdate -= WaveUpdate;
    }


    public override void OnEncounterStateChange(EncounterState oldState, EncounterState newState)
    {
        base.OnEncounterStateChange(oldState, newState);
    }


    void WaveStart()
    {
        WaveData currentWave = waveData[waveIndex];
        if (currentWave.maxAmount == 0)
        {
            throw new System.Exception("ERROR: 0 ENEMIES IN WAVE");
        }
        spawnedEnemies.AddRange(EncounterManager.SpawnWave(currentWave, spawnPoints, transform.position));
    }

    void WaveUpdate()
    {
        for (int x = spawnedEnemies.Count - 1; x >= 0; --x)
        {
            GameObject g = spawnedEnemies[x];
            ///TODO figure out a more sophisticated callback for this
            if (g == null || g.Equals(null))
            {
                spawnedEnemies.RemoveAt(x);
            }
        }
        if (spawnedEnemies.Count == 0)
        {
            WaveEnd();
        }
    }

    void WaveEnd()
    {
        ++waveIndex;
        if (waveIndex >= waveData.Length)
        {
            EncounterManager.instance.EndEncounter();
        }
        else
        {
            WaveStart();
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
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
