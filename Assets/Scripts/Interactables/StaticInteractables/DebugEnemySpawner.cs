using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugEnemySpawner : BaseInteractable
{
    public EnemyType enemyToSpawn;
    public Vector3 offset = Vector3.right * 3;
    public Vector3 offsetPer = Vector3.right;
    protected override bool CanInteract()
    {
        return true;
    }

    protected override void PerformInteract(GameObject user)
    {
        EnemyManager.instance.SpawnEnemy(enemyToSpawn, transform.position + offset);
        offset += offsetPer;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + offset);
        Gizmos.DrawIcon(transform.position + offset, "spawnpointgizmo");
    }
}
