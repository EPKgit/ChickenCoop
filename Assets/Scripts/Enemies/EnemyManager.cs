using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    DEBUG,
    NORMAL,
    SHIELD,
    MAX,
}

public class EnemyManager : MonoSingleton<EnemyManager>
{
    public delegate GameObject EnemySpawnRoutineDelegate(EnemySpawnData data, Vector3 position);
    [System.Serializable]
    public class EnemySpawnData
    {
        [SerializeField]
        public GameObject prefab;
        [SerializeField]
        public GameObject spawningEffect;
        public EnemySpawnRoutineDelegate spawningRoutine;
    }
    public EnemySpawnData[] enemySpawnData;
    private List<PoolLoanToken> loanTokens;

    private List<EnemySpawnData> spawnedEnemies;

    protected override void OnCreation()
    {
        base.OnCreation();
        spawnedEnemies = new List<EnemySpawnData>();

        enemySpawnData[(int)EnemyType.DEBUG].spawningRoutine = SpawnDiggingEnemy;
        enemySpawnData[(int)EnemyType.NORMAL].spawningRoutine = SpawnDiggingEnemy;
        enemySpawnData[(int)EnemyType.SHIELD].spawningRoutine = SpawnDiggingEnemy;

        loanTokens = new List<PoolLoanToken>();
        for (int x = 0; x < enemySpawnData.Length; ++x)
        {
            if (enemySpawnData[x].spawningEffect != null)
            {
                loanTokens.Add(PoolManager.instance.RequestLoan(enemySpawnData[x].spawningEffect, 5, true));
            }
        }
    }

    public GameObject SpawnEnemy(EnemyType type, Vector3 position)
    {        
        if(enemySpawnData == null || enemySpawnData.Length < (int)type)
        {
            Debug.LogError("ERROR: Enemy Manager does not have requisite data to spawn enemy");
            return null;
        }
        EnemySpawnData data = enemySpawnData[(int)type];
        GameObject prefab = data.prefab;
        if(prefab == null)
        {
            Debug.LogError("ERROR: Enemy Manager does not have requisite data to spawn enemy");
            return null;
        }

        if(data.spawningRoutine != null)
        {
            return data.spawningRoutine(data, position);
        }
        EnemySpawnData newEnemy = new EnemySpawnData();
        newEnemy.prefab = Instantiate(prefab, position, Quaternion.identity);
        spawnedEnemies.Add(newEnemy);
        return newEnemy.prefab;
    }

    public void FinishSpawnRoutine(GameObject g)
    {
        int index = 0;
        foreach(EnemySpawnData esd in spawnedEnemies)
        {
            if(esd.prefab == g)
            {
                if(esd.spawningEffect != null)
                {
                    var vfxPoolable = esd.spawningEffect.GetComponent<VFXPoolable>();
                    if(vfxPoolable == null)
                    {
                        Destroy(esd.spawningEffect);
                    }
                    else
                    {
                        vfxPoolable?.StopParticlePlaying();
                    }
                }
                spawnedEnemies.RemoveAt(index);
                return;
            }
            ++index;
        }
    }

    private GameObject SpawnDiggingEnemy(EnemySpawnData data, Vector3 position)
    {
        EnemySpawnData newEnemy = new EnemySpawnData();
        newEnemy.prefab = Instantiate(data.prefab, position, Quaternion.identity);
        newEnemy.spawningEffect = PoolManager.instance.RequestObject(data.spawningEffect);
        newEnemy.spawningEffect.transform.position = position + Vector3.down * 0.8f;
        spawnedEnemies.Add(newEnemy);
        return newEnemy.prefab;
    }

    private void OnValidate() 
    {
        if(enemySpawnData != null && enemySpawnData.Length != (int)EnemyType.MAX)
        {
            Debug.LogError("ERROR: enemyPrefabs array is not of same size as EnemyType.MAX");
            Array.Resize(ref enemySpawnData, (int)EnemyType.MAX);
        }
    }
}
