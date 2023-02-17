﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalEnemy : BaseEnemy
{
    public override EnemyType type => EnemyType.NORMAL;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void SetEnemyEnabled(bool enabled)
    {
        base.SetEnemyEnabled(enabled);
        rb.velocity = Vector2.zero;
    }

    protected override void Update()
	{
        base.Update();
        if (!base.UpdateMovement() || !CanMove())
        {
            rb.velocity = Vector2.zero;
            return;
        }
        //Just walks towards the player
        Vector2 dir = (chosenPlayer.transform.position - transform.position).normalized;
        rb.velocity = dir * speed;
	}
}
