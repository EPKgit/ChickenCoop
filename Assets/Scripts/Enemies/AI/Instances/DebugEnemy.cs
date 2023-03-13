using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class DebugEnemy : BaseEnemy
{
    public override EnemyType type => EnemyType.DEBUG;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
	{
        //if (!base.UpdateMovement() || !CanMove())
        //{
        //}
	}
}
