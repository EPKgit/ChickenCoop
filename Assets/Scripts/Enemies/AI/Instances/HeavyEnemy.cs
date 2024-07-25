using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using EnemyBehaviourSyntax;

public class HeavyEnemy : BaseEnemy
{
    public override EnemyType type => EnemyType.HEAVY;

    protected override void Awake()
    {
        base.Awake();
        updateList.Add(new EnemyBehaviourAction().If(IsDisabled).Then(StopMovement).AndEnd());
        updateList.Add(new EnemyBehaviourAction().If(HasNoValidTarget).Then(StopMovement));
        updateList.Add(new EnemyBehaviourAction().If(IsKnockedBack).Then(KnockbackUpdate).AndEnd());
        updateList.Add(new EnemyBehaviourAction().Do(AnimationUpdate));
        updateList.Add(new EnemyBehaviourAction().If(CanMove).Then(Move).Else(StopMovement).AndEnd());

        movementSprings.Add(new MoveTowardsTargetPlayerSpring());
        movementSprings.Add(new SeperateEnemiesSpring());
    }
    protected override void OnEnable()
	{
        base.OnEnable();
    }

    protected override void OnDisable()
	{
        base.OnDisable();
    }
}
