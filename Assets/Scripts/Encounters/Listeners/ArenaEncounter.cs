using Encounters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class ArenaEncounter : IEncounterListener
{ 
    public Vector2[] spawnPoints;
    public EnemyWaveData[] waveData;

    private int waveIndex = 0;
    private List<GameObject> spawnedEnemies;

    public ArenaEncounter(Encounter encounter) : base(encounter)
    {
    }

    public override void OnTriggerEncounter()
    {
        spawnedEnemies = new List<GameObject>();
        EncounterManager.instance.OnEncounterUpdate += WaveUpdate;
        WaveStart();
    }

    public override void OnEndEncounter()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            Object.Destroy(enemy);
        }
        EncounterManager.instance.OnEncounterUpdate -= WaveUpdate;
    }

    void WaveStart()
    {
        EnemyWaveData currentWave = waveData[waveIndex];
        spawnedEnemies.AddRange(EncounterManager.SpawnWave(currentWave, spawnPoints, encounter.transform.position));
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

#if UNITY_EDITOR
    public override void OnDrawGizmosSelected(Encounter encounter)
    {
        if (spawnPoints == null)
        {
            return;
        }
        GizmoHelpers.GizmoColor color = new GizmoHelpers.GizmoColor();
        int index = 0;
        foreach (Vector2 v in spawnPoints)
        {
            Vector2 relativePos = v + new Vector2(encounter.transform.position.x, encounter.transform.position.y);
            Gizmos.DrawLine(encounter.transform.position, relativePos);
            Handles.Label(relativePos, $"SpawnPoint{index++}");
        }

        foreach(EnemyWaveData waveData in waveData)
        {
            color.Next();
            waveData.OnDrawGizmosSelected(encounter.transform.position, false);
        }
    }
#endif
}