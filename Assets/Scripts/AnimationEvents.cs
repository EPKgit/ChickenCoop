using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void FinishSpawning()
    {
        GetComponent<SpriteRenderer>().material.SetFloat("SpawnPercent", 1.0f);
        EnemyManager.instance.FinishSpawnRoutine(transform.parent.gameObject);
        Lib.FindUpwardsInTree<BaseEnemy>(gameObject)?.SetEnemyEnabled(true);
        Lib.FindUpwardsInTree<TargetingController>(gameObject)?.SetTargetingEnabled(true);
    }

    public void StartSpawning()
    {
        Lib.FindUpwardsInTree<TargetingController>(gameObject)?.SetTargetingEnabled(false);
        BaseEnemy enemy = Lib.FindUpwardsInTree<BaseEnemy>(gameObject);
        enemy.SetEnemyEnabled(false);
        if (!enemy.isRegistered)
        {
            animator.SetBool("skipSpawnAnim", true);
           FinishSpawning();
        }
    }
}
