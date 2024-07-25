using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    DEBUG,
    NORMAL,
    SHIELD,
    HEAVY,
    MAX,
}

public class EnemyManager : MonoSingleton<EnemyManager>
{
    [System.Serializable]
    public class EnemySpawnData
    {
        [SerializeField]
        public GameObject prefab;
        [SerializeField]
        public GameObject spawningEffect;
        public Func<EnemySpawnData, Vector3, GameObject> spawningRoutine;
    }

    public class RegisteredEnemyData
    {
        public GameObject enemyGameObject;
        public BaseEnemy enemyScript;
        public EnemyType type;
    }
    public EnemySpawnData[] enemySpawnData;
    private List<PoolLoanToken> loanTokens;

    private List<EnemySpawnData> spawningEnemies;
    private List<RegisteredEnemyData> registeredEnemies;
    private bool currentlySpawning; //only true in the exact moment that something is being spawned so that they can see themselves as registered

    protected override void OnCreation()
    {
        base.OnCreation();
        spawningEnemies = new List<EnemySpawnData>();
        registeredEnemies = new List<RegisteredEnemyData>();

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

        subscribedTagComponents = new List<PlayerInitialization>();
        PlayerInitialization.OnPlayerNumberChanged += OnPlayerCountChanged;
        OnPlayerCountChanged();
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
            currentlySpawning = true;
            var returnVal = data.spawningRoutine(data, position);
            currentlySpawning = false;
            return returnVal;
        }
        EnemySpawnData newEnemy = new EnemySpawnData();
        currentlySpawning = true;
        newEnemy.prefab = Instantiate(prefab, position, Quaternion.identity); ;
        spawningEnemies.Add(newEnemy);
        currentlySpawning = false;
        return newEnemy.prefab;
    }

    public bool IsRegistered(GameObject g)
    {
        if(currentlySpawning)
        {
            return true;
        }
        foreach (EnemySpawnData esd in spawningEnemies)
        {
            if(esd.prefab == g)
            {
                return true;
            }
        }
        foreach(RegisteredEnemyData esd in registeredEnemies)
        {
            if(esd.enemyGameObject == g)
            {
                return true;
            }
        }
        return false;
    }

    public void RegisterEnemy(GameObject g)
    {
        if(g == null || IsRegistered(g))
        {
            return;
        }
        RegisteredEnemyData newEnemy = new RegisteredEnemyData();
        newEnemy.enemyGameObject = g;
        newEnemy.enemyScript = g.GetComponent<BaseEnemy>();
        newEnemy.type = newEnemy.enemyScript.type;
        registeredEnemies.Add(newEnemy);
    }

    public void UnregisterEnemy(GameObject g)
    {
        int x = 0;
        foreach(RegisteredEnemyData data in registeredEnemies)
        {
            if(data.enemyGameObject == g)
            {
                registeredEnemies.RemoveAt(x);
                return;
            }
            ++x;
        }
    }

    public void ForceReevaluateEnemyAggro()
    {
        foreach(RegisteredEnemyData data in registeredEnemies)
        {
            data.enemyGameObject.GetComponent<BaseEnemy>().UpdateChosenPlayer();
        }
    }

    public void FinishSpawnRoutine(GameObject g)
    {
        int index = 0;
        foreach(EnemySpawnData esd in spawningEnemies)
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
                spawningEnemies.RemoveAt(index);
                RegisterEnemy(esd.prefab);
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
        spawningEnemies.Add(newEnemy);
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

    private List<PlayerInitialization> subscribedTagComponents;
    public void OnPlayerCountChanged()
    {
        foreach(var player in PlayerInitialization.all)
        {
            GameplayTagComponent tagComp = player.GetComponent<GameplayTagComponent>();
            if(tagComp == null)
            {
                continue;
            }
            if(subscribedTagComponents.Contains(player))
            {
                tagComp.tags.OnTagChanged -= OnPlayerInvisibilityChanges;
                subscribedTagComponents.Remove(player);
            }
            tagComp.tags.OnTagChanged += OnPlayerInvisibilityChanges;
            subscribedTagComponents.Add(player);
        }
    }

    public void OnPlayerInvisibilityChanges(GameplayTagInternals.GameplayTagWrapper tag)
    {
        if(tag.Contains(GameplayTagFlags.INVISIBLE))
        {
            ForceReevaluateEnemyAggro();
        }
    }
}
