using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    public void FinishSpawning()
    {
        GetComponent<SpriteRenderer>().material.SetFloat("SpawnPercent", 1.0f);
        EnemyManager.instance.FinishSpawnRoutine(transform.parent.gameObject);        
        Lib.FindUpwardsInTree<BaseEnemy>(gameObject)?.SetEnemyEnabled(true);
        Lib.FindUpwardsInTree<TargetingController>(gameObject)?.SetTargetingEnabled(true);
    }

    public void StartSpawning()
    {
        Lib.FindUpwardsInTree<BaseEnemy>(gameObject)?.SetEnemyEnabled(false);
        Lib.FindUpwardsInTree<TargetingController>(gameObject)?.SetTargetingEnabled(false);
    }
}