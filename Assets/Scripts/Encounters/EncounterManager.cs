using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Encounters;
using System.Linq;
using System.Data;

public class EncounterManager : MonoSingleton<EncounterManager>
{
    public delegate void EncounterStateChangedDelegate(EncounterState oldState, EncounterState newState);
    public event EncounterStateChangedDelegate OnEncounterStateChange = delegate { };
    public event Action<string> OnEncounterEventBroadcast = delegate { };
    public event Action OnEncounterUpdate = delegate { };

    private EncounterState currentEncounterState;

    protected override void OnCreation()
    {
        base.OnCreation();
        currentEncounterState = EncounterState.NOT_IN_ENCOUNTER;
    }

    void Update()
    {
        if(currentEncounterState == EncounterState.NOT_IN_ENCOUNTER)
        {
            return;
        }
        OnEncounterUpdate();
    }

    public static List<GameObject> SpawnWave(WaveData data, Vector2[] spawnPoints, Vector3 offset)
    {
        List<GameObject> spawnedEnemies = new List<GameObject>();
        GrabBag<int> spawnBag = null;
        int spawnIndex = 0;
        if(data.spawnType == SpawnType.RANDOM)
        {
            spawnBag = new GrabBag<int>(Enumerable.Range(0, spawnPoints.Length - 1).ToArray(), true);
        }
        for (int x = 0; x < data.maxAmount; ++x)
        {
            Vector3 position;
            switch(data.spawnType)
            {
                case SpawnType.RANDOM:
                    position = spawnPoints[spawnBag.Grab()];
                    break;
                case SpawnType.SEQUENTIAL:
                    position = spawnPoints[spawnIndex++ % spawnPoints.Length];
                    break;
                default:
                    throw new System.Exception();
            }
            position += offset;
            spawnedEnemies.Add(EnemyManager.instance.SpawnEnemy(data.enemyType, position));
        }
        return spawnedEnemies;
    }

    public void StartEncounter()
    {
        if(currentEncounterState != EncounterState.NOT_IN_ENCOUNTER)
        {
            throw new System.Exception("ERROR: ATTEMPT TO START ENCOUNTER WHEN ONE IS ALREADY STARTED");
        }
        SetEncounterState(EncounterState.ENCOUNTER_STARTED);
    }

    public void EndEncounter()
    {
        SetEncounterState(EncounterState.ENCOUNTER_ENDED);
        SetEncounterState(EncounterState.NOT_IN_ENCOUNTER);
    }

    void SetEncounterState(EncounterState newState)
    {
        OnEncounterStateChange(currentEncounterState, newState);
        currentEncounterState = newState;
    }

    public EncounterState GetEncounterState()
    {
        return currentEncounterState;
    }
}
