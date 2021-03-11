using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
	public float movementSpeed;

	public Vector2 direction;

	private Rigidbody2D rb;
	private StatBlockComponent stats;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		stats = GetComponent<StatBlockComponent>();
		stats?.RegisterStatChangeCallback(StatName.Agility, UpdateSpeed);
	}

	void OnDisable()
	{
		stats?.DeregisterStatChangeCallback(StatName.Agility, UpdateSpeed);
	}

	void Update()
	{
		if(rb.velocity.magnitude < movementSpeed * 1.05f)
		{
			rb.velocity = direction * movementSpeed;
		}
		else
		{
			float degrees = Vector2.Angle(rb.velocity, direction);
			if (150 > degrees)
			{
				DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.MOVEMENT, "adding to force");
				rb.AddForce(direction * movementSpeed * 0.4f);
			}
			else
			{
                DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.MOVEMENT, "opossing");
				rb.AddForce(direction * movementSpeed * 1.5f);
			}
		}
	}

	public void Move(Vector2 dir)
	{
		direction = dir;
	}

	public void UpdateSpeed(float f)
	{
		movementSpeed = f;
	}
}
