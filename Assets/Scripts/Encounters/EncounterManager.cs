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

    public static List<GameObject> SpawnWave(EnemyWaveData waveData, Vector2[] generalSpawnPoints, Vector3 offset)
    {
        if(waveData.spawns == null || waveData.spawns.Length == 0)
        {
            throw new ArgumentException();
        }
        List<GameObject> spawnedEnemies = new List<GameObject>();
        GrabBag<int> spawnBag = null;
        int spawnIndex = 0;
        foreach (EnemyWaveData.EnemySpawnData spawnWithinWave in waveData.spawns)
        {
            SpawnData spawnData = spawnWithinWave.spawnData;
            if (spawnData.spawnType == SpawnType.USE_GENERAL_RANDOM)
            {
                spawnBag = new GrabBag<int>(Enumerable.Range(0, generalSpawnPoints.Length - 1).ToArray(), true);
            }
            else if (spawnData.spawnType == SpawnType.USE_GENERAL_RANDOM)
            {
                spawnBag = new GrabBag<int>(Enumerable.Range(0, spawnData.spawnPoints.Length - 1).ToArray(), true);
            }

            for (int x = 0; x < (spawnData.spawnType == SpawnType.SINGLE_POINT ? 1 : spawnData.spawnCount); ++x)
            {
                Vector3 position;
                switch (spawnData.spawnType)
                {
                    case SpawnType.USE_GENERAL_RANDOM:
                        position = generalSpawnPoints[spawnBag.Grab()];
                        break;
                    case SpawnType.RANDOM_OVER_POINTS:
                        position = spawnData.spawnPoints[spawnBag.Grab()];
                        break;
                    case SpawnType.SINGLE_POINT:
                        position = spawnData.spawnPoint;
                        break;
                    case SpawnType.SEQUENTIAL_OVER_POINTS:
                        position = spawnData.spawnPoints[spawnIndex++ % generalSpawnPoints.Length];
                        break;
                    case SpawnType.RANDOM_IN_AREA:
                        float randX = UnityEngine.Random.Range(0, spawnData.spawnArea.width);
                        float randY = UnityEngine.Random.Range(0, spawnData.spawnArea.height);
                        position = new Vector2(spawnData.spawnArea.x + randX, spawnData.spawnArea.y + randY);
                        break;
                    default:
                        throw new System.Exception();
                }
                position += offset;
                spawnedEnemies.Add(EnemyManager.instance.SpawnEnemy(spawnWithinWave.enemyType, position));
            }
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
