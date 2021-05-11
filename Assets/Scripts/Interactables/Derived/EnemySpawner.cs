using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : BaseInteractable
{
    public GameObject enemyToSpawn;
    public Vector3 offset = Vector3.right * 3;
    protected override bool CanDo()
    {
        return true;
    }

    protected override void ToDo(GameObject user)
    {
        Instantiate(enemyToSpawn, transform.position + offset, Quaternion.identity);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + offset);
    }
}
