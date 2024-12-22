using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Hitbox : Poolable
{
    public static Color debugColorNormal = new Color(1.0f, 0.0f, 0.0f, 0.4f);
    public static Color debugColorDelayed = new Color(1.0f, 1.0f, 0.0f, 0.4f);

    private static Collider2D[] collisions = new Collider2D[20];
    
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
            Vector2[] scaledPoints = new Vector2[data.Points.Length];
            for (int i = 0; i < data.Points.Length; ++i)
            {
                scaledPoints[i] = data.Points[i] * data.Scale;
            }
            polyCollider.points = scaledPoints;
            polyCollider.enabled = true;
        }
        else
        {
            polyCollider.enabled = false;
        }

        if (data.InteractionTimeStamps != null)
        {
            interactionTimestamps = data.InteractionTimeStamps;
        }

        SetActive(true);
    }

    public bool UpdateHitbox()
    {
        if (data.UpdateCallback != null)
        {
            data.UpdateCallback(this);
        }

        if(!active)
        {
            return true;
        }

        if(data.IsDelayed())
        {
            data.TickDelay(Time.deltaTime);
            return true;
        }

        if(!data.TickDuration(Time.deltaTime))
        {
            return false;
        }

        if(GatherCollisionCandidates())
        {
            ResolveCollisions();
        }
        return true;
    }

    bool GatherCollisionCandidates()
    {
        int amount = 0;
        switch (data.ShapeType)
        {
            case HitboxShapeType.CIRCLE:
                amount = Physics2D.OverlapCircleNonAlloc(transform.position, data.Radius * data.Scale, collisions, data.LayerMask.value);
                break;
            case HitboxShapeType.SQUARE:
                amount = Physics2D.OverlapBoxNonAlloc(transform.position, new Vector2(data.Radius * data.Scale * 2, data.Length * data.Scale * 2), data.StartRotationZ, collisions, data.LayerMask.value);
                break;
            case HitboxShapeType.PROJECTED_RECT:
                Vector2 center = (Vector2)transform.position + (data.Axis * data.Length * data.Scale * 0.5f);
                amount = Physics2D.OverlapBoxNonAlloc(center, new Vector2(data.Radius * data.Scale, data.Length * data.Scale), data.StartRotationZ, collisions, data.LayerMask.value);
                break;
            case HitboxShapeType.POLYGON:
                ContactFilter2D filter = new ContactFilter2D();
                filter.SetLayerMask(data.LayerMask);
                filter.useTriggers = true;
                amount = Physics2D.OverlapCollider(polyCollider, filter, collisions);
                break;
            default:
                Debug.LogError("ERROR: HURTBOX UPDATED WITH INCORRECT TYPE INFORMATION");
                return false;
        }

        if(amount <= 0)
        {
            return false;
        }

        int index = 0;
        foreach (Collider2D col in collisions)
        { 
            if (col != null && (data.Discriminator != null && !data.Discriminator(col)))
            {
                collisions[index] = null;
            }
            ++index;
        }
        return true;
    }

    void ResolveCollisions()
    {
        // check if our interaction timestamps are not in this frames info, if so call the leave collisions callback
        float closestCenterDistSq = float.MaxValue;
        Collider2D bestOption = null;
        foreach (var col in collisions)
        {
            if(col == null || !PassesRepeatPolicy(col))
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
                points[0] = transform.position + transform.rotation * new Vector3(-data.Radius * data.Scale, -data.Length * data.Scale);
                points[1] = transform.position + transform.rotation * new Vector3( data.Radius * data.Scale, -data.Length * data.Scale);
                points[2] = transform.position + transform.rotation * new Vector3( data.Radius * data.Scale,  data.Length * data.Scale);
                points[3] = transform.position + transform.rotation * new Vector3(-data.Radius * data.Scale,  data.Length * data.Scale);
                UnityEditor.Handles.DrawAAConvexPolygon(points);
            } break;

            case HitboxShapeType.PROJECTED_RECT:
            {
                var points = new Vector3[4];
                points[0] = transform.position + transform.rotation * new Vector3(-data.Radius * data.Scale, 0);
                points[1] = transform.position + transform.rotation * new Vector3( data.Radius * data.Scale, 0);
                points[2] = transform.position + transform.rotation * new Vector3( data.Radius * data.Scale, data.Length * data.Scale);
                points[3] = transform.position + transform.rotation * new Vector3(-data.Radius * data.Scale, data.Length * data.Scale);
                UnityEditor.Handles.DrawAAConvexPolygon(points);
            }
            break;

            case HitboxShapeType.POLYGON:
            { 
                var points = new Vector3[data.Points.Length];
                int x = 0;
                foreach(Vector2 v in data.Points)
                {
                    points[x] = (transform.rotation * (Vector3)(data.Points[x++] * data.Scale)) + transform.position;
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
                GLHelpers.GLDrawCircle(transform, data.Radius * data.Scale, 32, color);
                break;
            case HitboxShapeType.SQUARE:
            {
                Rect rect = new Rect(0, 0, data.Radius * data.Scale * 2, data.Length * data.Scale * 2);
                GLHelpers.GLDrawRect(transform, rect, color);
            } break;
            case HitboxShapeType.PROJECTED_RECT:
            {
                Rect rect = new Rect(0, 0, data.Radius * data.Scale * 2, data.Length * data.Scale);
                rect.center = data.Axis * 0.5f * data.Length * data.Scale;
                GLHelpers.GLDrawRect(transform, rect, color);
            } break;
            case HitboxShapeType.POLYGON:
                var points = new Vector3[data.Points.Length];
                for(int i = 0; i < data.Points.Length; ++i)
                {
                    points[i] = data.Points[i] * data.Scale;
                }
                GLHelpers.GLDrawPolygon(transform, points, color);
                break;
        }
    }
}
