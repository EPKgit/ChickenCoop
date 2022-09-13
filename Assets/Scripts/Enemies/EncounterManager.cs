using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnType
{
    RANDOM,
    SEQUENTIAL,
    MAX,
}

[System.Serializable]
public class WaveData
{
    public EnemyType enemyType;
    public int maxAmount;
    public SpawnType spawnType;
}


[System.Serializable]
public class EncounterData
{
    public WaveData[] waves;
}

public enum EncounterState
{
    NOT_IN_ENCOUNTER,
    ENCOUNTER_STARTED,
    WAVE_STARTED,
    WAVE_ENDED,
    ENCOUNTER_ENDED,
}

public class EncounterManager : MonoSingleton<EncounterManager>
{
    private class ActiveEncounterData
    {
        public EncounterData encounterData;
        public int waveIndex;
        public int spawnIndex;
        public List<Vector3> spawnPositions;

        public List<GameObject> spawnedEnemies;

        public ActiveEncounterData(EncounterData data, List<Vector3> spawns)
        {
            encounterData = data;
            waveIndex = 0;
            spawnIndex = 0;
            spawnedEnemies = new List<GameObject>();
            spawnPositions = spawns;
        }
    }

    public delegate void EncounterStateChangedDelegate(EncounterState oldState, EncounterState newState);
    public event EncounterStateChangedDelegate OnEncounterStateChange = delegate { };

    private ActiveEncounterData currentEncounter;

    private EncounterState currentEncounterState;

    protected override void OnCreation()
    {
        base.OnCreation();
        currentEncounter = null;
    }

    void Update()
    {
        if(currentEncounter == null)
        {
            return;
        }
        WaveUpdate();
    }

    void WaveStart()
    {
        SetEncounterState(EncounterState.WAVE_STARTED);
        WaveData currentWave = currentEncounter.encounterData.waves[currentEncounter.waveIndex];
        if(currentWave.maxAmount == 0)
        {
            throw new System.Exception("ERROR: 0 ENEMIES IN WAVE");
        }
        for (int x = 0; x < currentWave.maxAmount; ++x)
        {
            SpawnSingleEnemy(currentWave);
        }
    }

    Vector3 GetSpawnPosition(SpawnType type)
    {
        switch(type)
        {
            case SpawnType.RANDOM:
            {
                return currentEncounter.spawnPositions[Random.Range(0, currentEncounter.spawnPositions.Count)];
            }
            case SpawnType.SEQUENTIAL:
            {
                return currentEncounter.spawnPositions[currentEncounter.spawnIndex++ % currentEncounter.spawnPositions.Count];
            }
            default:
            {
                return Vector3.zero;
            }
        }
    }
    void SpawnSingleEnemy(WaveData data)
    {
        Vector3 position = GetSpawnPosition(data.spawnType);
        GameObject newEnemy = EnemyManager.instance.SpawnEnemy(data.enemyType, position);
        currentEncounter.spawnedEnemies.Add(newEnemy);
    }

    void WaveUpdate()
    {
        for (int x = currentEncounter.spawnedEnemies.Count - 1; x >= 0; --x)
        {
            GameObject g = currentEncounter.spawnedEnemies[x];
            ///TODO figure out a more sophisticated callback for this
            if(g == null || g.Equals(null))
            {
                currentEncounter.spawnedEnemies.RemoveAt(x);
            }
        }
        if(currentEncounter.spawnedEnemies.Count == 0)
        {
            WaveEnd();
        }
    }

    void WaveEnd()
    {
        SetEncounterState(EncounterState.WAVE_ENDED);
        ++currentEncounter.waveIndex;
        if(currentEncounter.waveIndex >= currentEncounter.encounterData.waves.Length)
        {
            EndEncounterInternal();
        }
        else
        {
            WaveStart();
        }
    }

    public void StartEncounter(EncounterData encounter, List<Vector2> spawnPositions, Vector2 offset = default(Vector2))
    {
        List<Vector3> temp = new List<Vector3>(spawnPositions.Count);
        foreach(Vector2 v in spawnPositions)
        {
            temp.Add(new Vector3(v.x + offset.x, v.y + offset.y, 0));
        }
        StartEncounter(encounter, temp);
    }

    public void StartEncounter(EncounterData encounter, List<Vector3> spawnPositions)
    {
        if(currentEncounter != null)
        {
            throw new System.Exception("ERROR: ATTEMPT TO START ENCOUNTER WHEN ONE IS ALREADY STARTED");
        }
        currentEncounter = new ActiveEncounterData(encounter, spawnPositions);
        SetEncounterState(EncounterState.ENCOUNTER_STARTED);
        WaveStart();
    }

    public void EndEncounter(EncounterData encounter)
    {
        if(currentEncounter.encounterData == encounter)
        {
            EndEncounterInternal();
        }
    }

    void EndEncounterInternal()
    {
        SetEncounterState(EncounterState.ENCOUNTER_ENDED);
        foreach (GameObject g in currentEncounter.spawnedEnemies)
        {
            Destroy(g);
        }
        currentEncounter = null;
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
