using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : Poolable
{
    public HitboxData data;

    private bool active;
    private Dictionary<GameObject, float> interactionTimestamps;
    void Awake()
    {
        Reset();
    }

    public override void Reset()
    {
        active = false;
        interactionTimestamps = new Dictionary<GameObject, float>();
    }

    public void SetActive(bool b)
    {
        active = b;
        if(active)
        {
            UpdateHitbox();
        }
    }

    public void Setup(HitboxData d)
    {
        data = d;
        SetActive(true);
    }

    public bool UpdateHitbox()
    {
        if(!active)
        {
            return true;
        }
        if(!data.TickDuration(Time.deltaTime))
        {
            return false;
        }
        List<Collider2D> collisions = GatherCollisionCandidates();
        if(collisions == null)
        {
            return true;
        }
        ResolveCollisions(collisions);
        return true;
    }

    List<Collider2D> GatherCollisionCandidates()
    {
        Collider2D[] overlaps = null;
        switch (data.Shape)
        {
            case HitboxShape.CIRCLE:
                overlaps = Physics2D.OverlapCircleAll(transform.position, data.Radius, data.LayerMask.value);
                break;
            case HitboxShape.SQUARE:
                overlaps = Physics2D.OverlapBoxAll(transform.position, new Vector2(data.Radius * 2, data.Radius * 2), 0, data.LayerMask.value);
                break;
            default:
                Debug.LogError("ERROR: HURTBOX UPDATED WITH INCORRECT TYPE INFORMATION");
                return null;
        }
        if(data.Discriminator == null)
        {
            return new List<Collider2D>(overlaps);
        }
        else
        {
            List<Collider2D> candidates = new List<Collider2D>();
            foreach (Collider2D col in overlaps)
            { 
                if(data.Discriminator(col))
                {
                    candidates.Add(col);
                }
            }
            return candidates;
        }
    }

    void ResolveCollisions(List<Collider2D> collisions)
    {
        float closestCenterDistSq = float.MaxValue;
        Collider2D bestOption = null;
        foreach (var col in collisions)
        {
            if(!PassesRepeatPolicy(col))
            {
                continue;
            }
            switch(data.InteractionType)
            {
                case HitboxInteractionType.FIRST:
                    ApplyCollisionInteraction(col);
                    return;
                case HitboxInteractionType.CLOSEST:
                    float distSq = (col.gameObject.transform.position - transform.position).sqrMagnitude;
                    if(distSq < closestCenterDistSq)
                    {
                        bestOption = col;
                        closestCenterDistSq = distSq;
                    }
                    break;
                case HitboxInteractionType.ALL:
                    ApplyCollisionInteraction(col);
                    break;
            }
        }
        if(bestOption != null)
        {
            ApplyCollisionInteraction(bestOption);
        }
    }

    bool PassesRepeatPolicy(Collider2D col)
    {
        switch(data.RepeatPolicy)
        {
            case HitboxRepeatPolicy.ONLY_ONCE:
                return !interactionTimestamps.ContainsKey(col.gameObject);
            case HitboxRepeatPolicy.COOLDOWN:
                if(interactionTimestamps.ContainsKey(col.gameObject) && (Time.time - interactionTimestamps[col.gameObject] < data.RepeatCooldown))
                {
                    return false;
                }
                return true;
            case HitboxRepeatPolicy.UNLIMITED:
                return true;
            default:
                throw new System.Exception("ERROR: Invalid HitboxRepeatPolicy");
        }
    }

    void ApplyCollisionInteraction(Collider2D col)
    {
        if(interactionTimestamps.ContainsKey(col.gameObject))
        {
            interactionTimestamps[col.gameObject] = Time.time;
        }
        else
        {
            interactionTimestamps.Add(col.gameObject, Time.time);
        }
        data.OnCollision(col);
    }

#pragma warning disable 162 //depending on the debug flag one of these is unreachable
    void OnDrawGizmosSelected()
    {
        if ((int)DEBUGFLAGS.FLAGS.HITBOXES == 0 && data != null)
        {
            DrawGizmo();
        }
    }

    void OnDrawGizmos()
    {
        if((int)DEBUGFLAGS.FLAGS.HITBOXES == 1 && data != null)
        {
            DrawGizmo();
        }
    }
#pragma warning restore 162

    void DrawGizmo()
    {
        UnityEditor.Handles.color = Color.red;
        switch (data.Shape)
        {
            case HitboxShape.CIRCLE:
                UnityEditor.Handles.DrawSolidDisc(transform.position, transform.forward, data.Radius);
                break;
            case HitboxShape.SQUARE:
                Rect rect = new Rect(0, 0, data.Radius * 2, data.Radius * 2);
                rect.center = transform.position;
                UnityEditor.Handles.DrawSolidRectangleWithOutline(rect, Color.red, Color.red);
                break;
        }
    }
}
