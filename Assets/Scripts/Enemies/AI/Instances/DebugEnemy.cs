using EnemyBehaviourSyntax;
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
        updateList.Add(new EnemyBehaviourAction().Do(StopMovement));
    }
}
