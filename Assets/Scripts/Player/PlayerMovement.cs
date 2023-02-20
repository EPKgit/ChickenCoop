﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : BaseMovement
{
	private Vector2 movementInputAxis;

	protected override void Update()
	{
        base.Update();
        if(!CanMove())
        {
            rb.velocity = Vector2.zero;
            return;
        }
        CheckKnockbackInput();
        if(tagComponent.tags.Contains(GameplayTagFlags.KNOCKBACK))
        {
            DebugFlags.Log(DebugFlags.Flags.MOVEMENT, "MOVEMENT CANCELED FROM KNOCKBAC");
            return;
        }
        CheckDashInput();
        if(tagComponent.tags.Contains(GameplayTagFlags.NORMAL_MOVEMENT_DISABLED))
        {
            DebugFlags.Log(DebugFlags.Flags.MOVEMENT, "MOVEMENT CANCELED FROM TAG");
            return;
        }
        UseMoveInput();
    }

    void UseMoveInput()
    {
        if (rb.velocity.magnitude < movementSpeed * 1.05f)
        {
            DebugFlags.Log(DebugFlags.Flags.MOVEMENT, "SETTING SPEED FROM INPUT");
            rb.velocity = movementInputAxis.normalized * movementSpeed;
        }
        else
        {
            float degrees = Vector2.Angle(rb.velocity, movementInputAxis);
            if (150 > degrees)
            {
                DebugFlags.Log(DebugFlags.Flags.MOVEMENT, "ADDING TO OVER MOVEMENT FORCE");
                rb.AddForce(movementInputAxis * movementSpeed * 0.4f);
            }
            else
            {
                DebugFlags.Log(DebugFlags.Flags.MOVEMENT, "OPPOSING OVER MOVEMENT FORCE");
                rb.AddForce(movementInputAxis * movementSpeed * 1.5f);
            }
        }
        movedDuringFrame = movementInputAxis.sqrMagnitude > float.Epsilon;
        animator.SetBool(animatorMovingHashCode, movedDuringFrame);
        if (movedDuringFrame)
        {
            sprite.flipX = rb.velocity.x < 0;
        }
    }

    public void MoveInput(Vector2 dir)
	{
		movementInputAxis = dir;
	}
}