using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemyBehaviourSyntax;

public class NormalEnemy : BaseEnemy
{
    public override EnemyType type => EnemyType.NORMAL;

    protected override void Awake()
    {
        base.Awake();
        updateList.Add(new EnemyBehaviourAction().If(IsDisabled).Then(StopMovement).AndEnd());
        updateList.Add(new EnemyBehaviourAction().If(HasNoValidTarget).Then(StopMovement).AndEnd());
        updateList.Add(new EnemyBehaviourAction().If(IsKnockedBack).Then(KnockbackUpdate).AndEnd());
        updateList.Add(new EnemyBehaviourAction().Do(AnimationUpdate));
        updateList.Add(new EnemyBehaviourAction().If(CanMove).Then(Move).Else(StopMovement).AndEnd());
    }
}
