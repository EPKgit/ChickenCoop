using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Encounters;
using UnityEditor;

[RequireComponent(typeof(Collider2D))]
public class LockedGatherEncounter : Encounter
{
    public Vector2[] spawnPoints;
    public GatheringWaveData[] waveData;

    private int waveIndex = 0;
    private List<GameObject> spawnedGatherables;

    //protected override void StartEncounter()
    //{
    //    spawnedGatherables = new List<GameObject>();
    //    EncounterManager.instance.OnEncounterUpdate += WaveUpdate;
    //    WaveStart();
    //}

    //protected override void Cleanup()
    //{
    //    foreach(GameObject gatherable in spawnedGatherables)
    //    {
    //        Destroy(gatherable);
    //    }
    //    EncounterManager.instance.OnEncounterUpdate -= WaveUpdate;
    //}


    //public override void OnEncounterStateChange(EncounterState oldState, EncounterState newState)
    //{
    //    base.OnEncounterStateChange(oldState, newState);
    //}


    //void WaveStart()
    //{
    //    GatheringWaveData currentWave = waveData[waveIndex];
    //    //if (currentWave.maxAmount == 0)
    //    //{
    //        //throw new System.Exception("ERROR: 0 ENEMIES IN WAVE");
    //    //}
    //    //spawnedGatherables.AddRange(EncounterManager.SpawnWave(currentWave, spawnPoints, transform.position));
    //}

    //void WaveUpdate()
    //{
    //    for (int x = spawnedGatherables.Count - 1; x >= 0; --x)
    //    {
    //        GameObject g = spawnedGatherables[x];
    //        ///TODO figure out a more sophisticated callback for this
    //        if (g == null || g.Equals(null))
    //        {
    //            spawnedGatherables.RemoveAt(x);
    //        }
    //    }
    //    if (spawnedGatherables.Count == 0)
    //    {
    //        WaveEnd();
    //    }
    //}

    //void WaveEnd()
    //{
    //    ++waveIndex;
    //    if (waveIndex >= waveData.Length)
    //    {
    //        EncounterManager.instance.EndEncounter();
    //    }
    //    else
    //    {
    //        WaveStart();
    //    }
    //}

    //protected override void OnDrawGizmosSelected()
    //{
    //    base.OnDrawGizmosSelected();
    //    if (spawnPoints == null)
    //    {
    //        return;
    //    }
    //    Gizmos.color = Color.red;
    //    int index = 0;
    //    foreach (Vector2 v in spawnPoints)
    //    {
    //        Vector2 relativePos = v + new Vector2(transform.position.x, transform.position.y);
    //        Gizmos.DrawLine(transform.position, relativePos);
    //        Handles.Label(relativePos, $"SpawnPoint{index++}");
    //    }
    //}
}
