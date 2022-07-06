using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementType
{
    NONE,
    NORMAL,
    DASH,
    TELEPORT,
    END_OF_FRAME_DELTA,
    MAX,
}

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public event MovementDeltaEventDelegate movementEvent = delegate { };

	private float movementSpeed;
	private Vector2 movementInputAxis;
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
        previousPosition = transform.position;
    }

	void OnDisable()
	{
		stats?.DeregisterStatChangeCallback(StatName.Agility, UpdateSpeed);
	}

	void Update()
	{
        if(tagComponent?.tags.Contains(GameplayTagFlags.MOVEMENT_DISABLED) ?? false)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        CheckDashInput();
        if(tagComponent?.tags.Contains(GameplayTagFlags.NORMAL_MOVEMENT_DISABLED) ?? false)
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

    private void LateUpdate()
    {
        movementEvent(new MovementDeltaEventData((Vector2)transform.position - previousPosition, MovementType.END_OF_FRAME_DELTA));
        previousPosition = transform.position;
    }

    public void MoveInput(Vector2 dir)
	{
		movementInputAxis = dir;
	}


    uint dashTagID = uint.MaxValue;
    Vector3 dashStart;
    Vector3 dashEnd;
    Vector3 dashDelta;
    float dashDuration;
    float dashStartTime;
    public void DashInput(Vector2 start, Vector2 end, float time)
    {
        dashTagID = tagComponent.tags.AddTag(GameplayTagFlags.MOVEMENT_DASHING);
        dashStartTime = Time.time;
        dashStart = start;
        dashEnd = end;
        dashDelta = dashEnd - dashStart;
        dashDuration = time;
        rb.velocity = dashDelta / dashDuration;
        rb.drag = 0;
    }

    void CheckDashInput()
    {
        if (dashDuration != 0)
        {
            Vector2 prevPosition = transform.position;
            float t = (Time.time - dashStartTime) / dashDuration;
            if (t > 1)
            {
                rb.velocity = Vector2.zero;
                sprite.transform.rotation = Quaternion.identity;
                dashDuration = 0;
                tagComponent.tags.RemoveTagWithID(dashTagID);
            }
            else
            {
                sprite.transform.rotation = Quaternion.AngleAxis(t * 360 * Mathf.Floor(dashDuration / 0.25f), (dashEnd - dashStart).x < 0 ? Vector3.forward : Vector3.back);
            }
            movementEvent(new MovementDeltaEventData(prevPosition - (Vector2)transform.position, MovementType.DASH));

            animator.SetBool(animatorMovingHashCode, false);
            sprite.flipX = dashDelta.x < 0;
        }
    }

    public void TeleportInput(Vector2 end)
    {
        Vector2 curr = transform.position;
        transform.position = end;
        movementEvent(new MovementDeltaEventData(curr - end, MovementType.TELEPORT));
    }

	public void UpdateSpeed(float f)
	{
		movementSpeed = f;
	}

}

public delegate void MovementDeltaEventDelegate(MovementDeltaEventData eventData);

public class MovementDeltaEventData
{
    public MovementType type;
    public Vector2 delta;
    public MovementDeltaEventData(Vector2 d, MovementType m)
    {
        type = m;
        delta = d;
    }
}