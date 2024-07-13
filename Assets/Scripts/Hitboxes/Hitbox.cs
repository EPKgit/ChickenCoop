using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Hitbox : Poolable
{
    public static Color debugColorNormal = new Color(1.0f, 0.0f, 0.0f, 0.4f);
    public static Color debugColorDelayed = new Color(1.0f, 1.0f, 0.0f, 0.4f);
    
    public HitboxData data;

    private bool active;
    private PolygonCollider2D polyCollider;
    private Dictionary<GameObject, float> interactionTimestamps;
    void Awake()
    {
        polyCollider = GetComponent<PolygonCollider2D>();
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
        if(!d.Validated)
        {
            throw new System.Exception("ERROR: Set HitboxData with invalid data");
        }
        data = d;
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            polyCollider = GetComponent<PolygonCollider2D>();
        }
#endif
        if (data.ShapeType == HitboxShapeType.POLYGON)
        {
            polyCollider.points = data.Points;
            polyCollider.enabled = true;
        }
        else
        {
            polyCollider.enabled = false;
        }

        if(data.InteractionTimeStamps != null)
        {
            interactionTimestamps = data.InteractionTimeStamps;
        }

        SetActive(true);
    }

    public bool UpdateHitbox()
    {
        if(!active)
        {
            return true;
        }

        if(data.IsDelayed())
        {
            data.TickDelay(Time.deltaTime);
            return true;
        }

        List<Collider2D> collisions = GatherCollisionCandidates();
        if(collisions == null)
        {
            return true;
        }
        ResolveCollisions(collisions);
        return data.TickDuration(Time.deltaTime);
    }

    List<Collider2D> GatherCollisionCandidates()
    {
        Collider2D[] overlaps = null;
        switch (data.ShapeType)
        {
            case HitboxShapeType.CIRCLE:    
                overlaps = Physics2D.OverlapCircleAll(transform.position, data.Radius, data.LayerMask.value);
                break;
            case HitboxShapeType.SQUARE:
                overlaps = Physics2D.OverlapBoxAll(transform.position, new Vector2(data.Radius * 2, data.Length * 2), data.StartRotationZ, data.LayerMask.value);
                break;
            case HitboxShapeType.PROJECTED_RECT:
                Vector2 center = (Vector2)transform.position + data.Axis * data.Length * 0.5f;
                overlaps = Physics2D.OverlapBoxAll(center, new Vector2(data.Radius, data.Length), data.StartRotationZ, data.LayerMask.value);
                break;
            case HitboxShapeType.POLYGON:
                overlaps = new Collider2D[8];
                ContactFilter2D filter = new ContactFilter2D();
                filter.SetLayerMask(data.LayerMask);
                filter.useTriggers = true;
                Physics2D.OverlapCollider(polyCollider, filter, overlaps);
                break;
            default:
                Debug.LogError("ERROR: HURTBOX UPDATED WITH INCORRECT TYPE INFORMATION");
                return null;
        }
        List<Collider2D> candidates = new List<Collider2D>();
        foreach (Collider2D col in overlaps)
        { 
            if (col != null && (data.Discriminator == null || data.Discriminator(col)))
            {
                candidates.Add(col);
            }
        }
        return candidates;
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
        data.OnCollision(col, this);
    }

#if UNITY_EDITOR

    //only appears in editor while we are previewing hitboxes
    [NonSerialized]
    public HitboxData previewData;
#pragma warning disable 162 //depending on the debug flag one of these is unreachable
    void OnDrawGizmosSelected()
    {
        if (data != null)
        {
            DrawGizmo(this.data);
        }

        if(previewData != null)
        {
            DrawGizmo(previewData);
        }
    }

    void OnDrawGizmos()
    {
        if((int)DebugFlags.Flags.HITBOXES_EDITOR_ALWAYS == 1 && data != null)
        {
            DrawGizmo(this.data);
        }
    }
#pragma warning restore 162

    public void DrawGizmo(HitboxData data)
    {
        UnityEditor.Handles.color = data.IsDelayed() ? debugColorDelayed : debugColorNormal;
        switch (data.ShapeType)
        {
            case HitboxShapeType.CIRCLE:
            {
                UnityEditor.Handles.DrawSolidDisc(transform.position, transform.forward, data.Radius);
            } break;
            case HitboxShapeType.SQUARE:
            {
                var points = new Vector3[4];
                points[0] = transform.rotation * new Vector3(-data.Radius, -data.Length) + transform.position;
                points[1] = transform.rotation * new Vector3( data.Radius, -data.Length) + transform.position;
                points[2] = transform.rotation * new Vector3( data.Radius,  data.Length) + transform.position;
                points[3] = transform.rotation * new Vector3(-data.Radius,  data.Length) + transform.position;
                UnityEditor.Handles.DrawAAConvexPolygon(points);
            } break;

            case HitboxShapeType.PROJECTED_RECT:
            {
                var points = new Vector3[4];
                points[0] = transform.rotation * new Vector3(-data.Radius, 0) + transform.position;
                points[1] = transform.rotation * new Vector3(data.Radius, 0) + transform.position;
                points[2] = transform.rotation * new Vector3(data.Radius, data.Length) + transform.position;
                points[3] = transform.rotation * new Vector3(-data.Radius, data.Length) + transform.position;
                UnityEditor.Handles.DrawAAConvexPolygon(points);
            }
            break;

            case HitboxShapeType.POLYGON:
            { 
                var points = new Vector3[data.Points.Length];
                int x = 0;
                foreach(Vector2 v in data.Points)
                {
                    points[x] = (transform.rotation * (Vector3)data.Points[x++]) + transform.position;
                }
                UnityEditor.Handles.DrawAAConvexPolygon(points);
            } break;

        }
    }
#endif

    private void OnRenderObject()
    {
        if ((int)DebugFlags.Flags.HITBOXES_IN_GAME == 1 && data != null && active)
        {
            DrawInGameDebug(this.data);
        }
    }

    public void DrawInGameDebug(HitboxData data)
    {
        var color = data.IsDelayed() ? debugColorDelayed : debugColorNormal;
        switch (data.ShapeType)
        {
            case HitboxShapeType.CIRCLE:
                GLHelpers.GLDrawCircle(transform, data.Radius, 32, color);
                break;
            case HitboxShapeType.SQUARE:
            {
                Rect rect = new Rect(0, 0, data.Radius * 2, data.Length * 2);
                GLHelpers.GLDrawRect(transform, rect, color);
            } break;
            case HitboxShapeType.PROJECTED_RECT:
            {
                Rect rect = new Rect(0, 0, data.Radius * 2, data.Length);
                rect.center = data.Axis * 0.5f * data.Length;
                GLHelpers.GLDrawRect(transform, rect, color);
            } break;
            case HitboxShapeType.POLYGON:
                var points = new Vector3[data.Points.Length];
                int x = 0;
                foreach (Vector2 v in data.Points)
                {
                    points[x] = data.Points[x++];
                }
                GLHelpers.GLDrawPolygon(transform, points, color);
                break;
        }
    }
}
