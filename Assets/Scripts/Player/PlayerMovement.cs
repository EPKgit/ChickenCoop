using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public event MovementDeltaEventDelegate movementEvent = delegate { };

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
        CheckDashInput();
        if(tagComponent?.tags.Contains(GameplayTagFlags.NORMAL_MOVEMENT_DISABLED) ?? false)
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
        UseMoveInput();
    }
    void CheckDashInput()
    {
        if (dashDuration != 0)
        {
            Vector2 prevPosition = transform.position;
            float t = (Time.time - dashStartTime) / dashDuration;
            if (t > 1)
            {
                transform.position = dashEnd;
                dashDuration = 0;
                tagComponent.tags.RemoveTagWithID(dashTagID);
            }
            else
            {
                transform.position = Vector2.Lerp(dashStart, dashEnd, t);
            }
            movementEvent(new MovementDeltaEventData(prevPosition - (Vector2)transform.position, MovementDeltaEventData.MovementType.DASH));

            return;
        }
    }

    void UseMoveInput()
    {
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
        movementEvent(new MovementDeltaEventData((Vector2)transform.position - previousPosition, MovementDeltaEventData.MovementType.TELEPORT));
        previousPosition = transform.position;
    }

    public void MoveInput(Vector2 dir)
	{
		direction = dir;
	}


    int dashTagID = -1;
    Vector3 dashStart;
    Vector3 dashEnd;
    float dashDuration;
    float dashStartTime;
    public void DashInput(Vector2 start, Vector2 end, float time)
    {
        dashTagID = tagComponent.tags.AddTag(GameplayTagFlags.MOVEMENT_DASHING);
        dashStartTime = Time.time;
        dashStart = start;
        dashEnd = end;
        dashDuration = time;
        rb.velocity = Vector2.zero;
    }

    public void TeleportInput(Vector2 end)
    {
        Vector2 curr = transform.position;
        transform.position = end;
        movementEvent(new MovementDeltaEventData(curr - end, MovementDeltaEventData.MovementType.TELEPORT));
    }

	public void UpdateSpeed(float f)
	{
		movementSpeed = f;
	}

}

public delegate void MovementDeltaEventDelegate(MovementDeltaEventData eventData);

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
    public MovementDeltaEventData(Vector2 d, MovementType m)
    {
        type = m;
        delta = d;
    }
}