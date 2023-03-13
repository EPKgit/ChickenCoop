using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemyBehaviourSyntax;

public class NormalEnemy : BaseEnemy
{
    public override EnemyType type => EnemyType.NORMAL;

    private List<EnemyBehaviourAction> updateList;

    protected override void Awake()
    {
        base.Awake();
        updateList = new List<EnemyBehaviourAction>();
        updateList.Add(new EnemyBehaviourAction().If(IsDisabled).Then(StopMovement).AndEnd());
        updateList.Add(new EnemyBehaviourAction().If(HasNoValidTarget).Then(StopMovement).AndEnd());
        updateList.Add(new EnemyBehaviourAction().If(IsKnockedBack).Then(KnockbackUpdate).AndEnd());
        updateList.Add(new EnemyBehaviourAction().Do(AnimationUpdate));
        updateList.Add(new EnemyBehaviourAction().If(CanMove).Then(Move).Else(StopMovement).AndEnd());
    }

    protected override void Update()
	{
        base.Update();
        ProcessUpdateList();
	}

    protected void ProcessUpdateList()
    {
        for(int x = 0; x < updateList.Count; ++x)
        {
            EnemyBehaviourAction action = updateList[x];
            if(!action.Evaluate())
            {
                break;
            }
        }
    }

    protected void Move()
    {
        //Just walks towards the player
        Vector2 dir = (chosenPlayer.transform.position - transform.position).normalized;
        rb.velocity = dir * speed;
    }

    protected void StopMovement()
    {
        rb.velocity = Vector2.zero;
    }
}
