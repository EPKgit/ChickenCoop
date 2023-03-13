using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementType
{
    NONE,
    NORMAL,
    DASH,
    TELEPORT,
    KNOCKBACK,
    END_OF_FRAME_DELTA,
    MAX,
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

[RequireComponent(typeof(Rigidbody2D), typeof(StatBlockComponent))]
public class BaseMovement : MonoBehaviour, IKnockbackHandler
{
    public Vector3 position => transform.position;

    public event MovementDeltaEventDelegate movementEvent = delegate { };

    public float movementSpeed = 1;
    public bool getsKnockbackInvuln = false;

    protected Vector2 previousPosition;
    protected bool movingAtStartOfFrame;
    protected bool movedDuringFrame;
    protected bool movingAtEndOfFrame;

    protected Rigidbody2D rb;
    protected StatBlockComponent stats;
    protected GameplayTagComponent tagComponent;
    protected SpriteRenderer sprite;
    protected Animator animator;
    protected int animatorMovingHashCode;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<StatBlockComponent>();
        if(stats.HasStat(StatName.MovementSpeed))
        {
            stats.GetStat(StatName.MovementSpeed).BaseValue = movementSpeed;
        }
        tagComponent = GetComponentInChildren<GameplayTagComponent>();
		sprite = GetComponentInChildren<SpriteRenderer>();
        previousPosition = transform.position;
        animator = GetComponentInChildren<Animator>();
        animatorMovingHashCode = Animator.StringToHash("moving");
    }

    protected virtual void Update()
    {
        movingAtStartOfFrame = rb.velocity.magnitude > float.Epsilon;
    }

    void OnEnable()
    {
        stats.RegisterStatChangeCallback(StatName.MovementSpeed, UpdateMovementSpeedStat);
    }

    void OnDisable()
    {
        stats?.DeregisterStatChangeCallback(StatName.MovementSpeed, UpdateMovementSpeedStat);
    }


    private void LateUpdate()
    {
        movingAtEndOfFrame = rb.velocity.magnitude > float.Epsilon;
        movementEvent(new MovementDeltaEventData((Vector2)transform.position - previousPosition, MovementType.END_OF_FRAME_DELTA));
        previousPosition = transform.position;
    }

    protected virtual bool CanMove()
    {
        if (tagComponent.tags.Contains(GameplayTagFlags.MOVEMENT_DISABLED))
        {
            return false;
        }
        return true;
    }

    GameplayTagInternals.GameplayTagID dashTagID;
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

    protected void CheckDashInput()
    {
        if (dashDuration != 0)
        {
            float t = (Time.time - dashStartTime) / dashDuration;
            if (t > 1)
            {
                rb.velocity = Vector2.zero;
                sprite.transform.rotation = Quaternion.identity;
                dashDuration = 0;
                tagComponent.tags.RemoveFirstTagWithID(dashTagID);
            }
            else
            {
                float angle = t * 360 * Mathf.Max(Mathf.Floor(dashDuration / 0.25f), 1);
                Vector3 axis = (dashEnd - dashStart).x < 0 ? Vector3.forward : Vector3.back;
                sprite.transform.rotation = Quaternion.AngleAxis(angle, axis);
            }
            movementEvent(new MovementDeltaEventData((Vector2)transform.position - previousPosition, MovementType.DASH));

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

    public void UpdateMovementSpeedStat(float f)
    {
        movementSpeed = f;
    }

    protected float knockbackStartTime;
    protected float knockbackDuration;
    protected GameplayTagInternals.GameplayTagID knockbackTagID;
    public virtual void ApplyKnockback(KnockbackData data)
    {
        if (tagComponent.tags.Contains(GameplayTagFlags.KNOCKBACK) || tagComponent.tags.Contains(GameplayTagFlags.KNOCKBACK_IMMUNITY))
        {
            return;
        }
        knockbackTagID = tagComponent.tags.AddTag(GameplayTagFlags.KNOCKBACK);
        knockbackStartTime = Time.time;
        knockbackDuration = data.duration;
        rb.velocity = (data.direction * data.force) / knockbackDuration;
        rb.drag = 0;
    }

    protected bool IsKnockedBack()
    {
        return knockbackDuration != 0;
    }

    protected void KnockbackUpdate()
    {
        float t = (Time.time - knockbackStartTime) / knockbackDuration;
        if (t > 1)
        {
            rb.velocity = Vector2.zero;
            if (getsKnockbackInvuln)
            {
                StatusEffectManager.instance.ApplyEffect(gameObject, Statuses.StatusEffectType.KNOCKBACK_IMMUNITY, 0.5f - knockbackDuration);
            }
            knockbackDuration = 0;
            tagComponent.tags.RemoveFirstTagWithID(knockbackTagID);
        }
        movementEvent(new MovementDeltaEventData((Vector2)transform.position - previousPosition, MovementType.KNOCKBACK));
    }

    protected void CheckKnockbackInput()
    {
        if(IsKnockedBack())
        {
            KnockbackUpdate();
        }
    }
}