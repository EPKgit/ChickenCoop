using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public MovementDeltaEventDelegate movementEvent;

	private float movementSpeed;
	private Vector2 direction;
    private Vector2 previousPosition;

	private Rigidbody2D rb;
	private StatBlockComponent stats;
    private Animator animator;
    private SpriteRenderer sprite;
    private GameplayTagComponent tagComponent;

    private int animatorMovingHashCode;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		stats = GetComponent<StatBlockComponent>();
		animator = GetComponentInChildren<Animator>();
		sprite = GetComponentInChildren<SpriteRenderer>();
		tagComponent = GetComponentInChildren<GameplayTagComponent>();
        animatorMovingHashCode = Animator.StringToHash("moving");
		stats?.RegisterStatChangeCallback(StatName.Agility, UpdateSpeed);
	}

	void OnDisable()
	{
		stats?.DeregisterStatChangeCallback(StatName.Agility, UpdateSpeed);
	}

	void Update()
	{
        if(tagComponent?.tags.Contains(GameplayTagFlags.MOVEMENT_DISABLED) ?? false)
        {
            DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.MOVEMENT, "MOVEMENT CANCELED FROM TAG");
            return;
        }
        bool moving = direction.magnitude > float.Epsilon;
        animator.SetBool(animatorMovingHashCode, moving);
        if (moving)
        {
            sprite.flipX = rb.velocity.x < 0;
        }
        if (rb.velocity.magnitude < movementSpeed * 1.05f)
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

    private void LateUpdate()
    {
        MovementDeltaEventData data = new MovementDeltaEventData();
        data.delta = (Vector2)transform.position - previousPosition;
        previousPosition = transform.position;
        data.type = MovementDeltaEventData.MovementType.NORMAL;
    }

    public void MoveInput(Vector2 dir)
	{
		direction = dir;
	}

    public void DashInput(Vector2 start, Vector2 end, float time)
    {

    }

	public void UpdateSpeed(float f)
	{
		movementSpeed = f;
	}

}

public delegate bool MovementDeltaEventDelegate(MovementDeltaEventData eventData);

public class MovementDeltaEventData
{
    public enum MovementType
    {
        NONE,
        NORMAL,
        DASH,
        TELEPORT,
        MAX,
    }
    public MovementType type;
    public Vector2 delta;
}