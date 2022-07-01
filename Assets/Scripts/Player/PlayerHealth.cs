using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : BaseHealth
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Die(GameObject killer = null)
    {
        base.Die();
        gameObject.transform.position = Vector3.zero;
    }
}
