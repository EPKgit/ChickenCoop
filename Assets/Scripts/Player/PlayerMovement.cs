using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
	public float movementSpeed;

	public Vector2 direction;

	private Rigidbody2D rb;
	private StatBlock stats;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		stats = GetComponent<StatBlock>();
		stats?.RegisterInitializationCallback(UpdateSpeed);
	}

	void OnDisable()
	{
		stats?.GetStat(StatName.Agility)?.UnregisterStatChangeCallback(UpdateSpeed);
		stats?.DeregisterInitializationCallback(UpdateSpeed);
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
				Debug.Log("adding to force");
				rb.AddForce(direction * movementSpeed * 0.4f);
			}
			else
			{
				Debug.Log("opossing");
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

	public void UpdateSpeed(StatBlock s)
	{
		stats = s;
		s.GetStat(StatName.Agility)?.RegisterStatChangeCallback(UpdateSpeed);
		movementSpeed = s?.GetStat(StatName.Agility)?.value ?? movementSpeed;
	}
}
