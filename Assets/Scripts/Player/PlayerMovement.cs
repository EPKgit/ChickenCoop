using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : BaseMovement
{
	private Vector2 movementInputAxis;

	protected override void Awake()
	{
        base.Awake();
        stats?.RegisterStatChangeCallback(StatName.Agility, UpdateSpeed);
    }

	void Update()
	{
        if(!CanMove())
        {
            rb.velocity = Vector2.zero;
            return;
        }
        CheckKnockbackInput();
        if(tagComponent.tags.Contains(GameplayTagFlags.KNOCKBACK))
        {
            DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.MOVEMENT, "MOVEMENT CANCELED FROM KNOCKBAC");
            return;
        }
        CheckDashInput();
        if(tagComponent.tags.Contains(GameplayTagFlags.NORMAL_MOVEMENT_DISABLED))
        {
            DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.MOVEMENT, "MOVEMENT CANCELED FROM TAG");
            return;
        }
        UseMoveInput();
    }

    void UseMoveInput()
    {
        if (rb.velocity.magnitude < movementSpeed * 1.05f)
        {
            DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.MOVEMENT, "SETTING SPEED FROM INPUT");
            rb.velocity = movementInputAxis.normalized * movementSpeed;
        }
        else
        {
            float degrees = Vector2.Angle(rb.velocity, movementInputAxis);
            if (150 > degrees)
            {
                DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.MOVEMENT, "ADDING TO OVER MOVEMENT FORCE");
                rb.AddForce(movementInputAxis * movementSpeed * 0.4f);
            }
            else
            {
                DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.MOVEMENT, "OPPOSING OVER MOVEMENT FORCE");
                rb.AddForce(movementInputAxis * movementSpeed * 1.5f);
            }
        }
        bool moving = movementInputAxis.magnitude > float.Epsilon;
        animator.SetBool(animatorMovingHashCode, moving);
        if (moving)
        {
            sprite.flipX = rb.velocity.x < 0;
        }
    }

    public void MoveInput(Vector2 dir)
	{
		movementInputAxis = dir;
	}
}