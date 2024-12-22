using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum HitboxShapeType
{
    CIRCLE,
    SQUARE,
    PROJECTED_RECT,
    POLYGON,
    MAX
}

public enum HitboxInteractionType
{
    FIRST,
    CLOSEST,
    ALL,
    MAX
}

public enum HitboxRepeatPolicy
{
    ONLY_ONCE,
    COOLDOWN,
    UNLIMITED,
    MAX
}
public class HitboxData
{ 
    public bool Validated { get; private set; } = false;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //These are what are necessary for a valid hitbox
    public Vector3 StartPosition { get; private set; } = Vector3.positiveInfinity;
    public Action<Collider2D, Hitbox> OnCollision { get; private set; }
    public Func<Collider2D, bool> Discriminator { get; private set; }


    //These have defaults and aren't necessary
    public Action<Hitbox> UpdateCallback { get; private set; } = null;
    public HitboxShapeType ShapeType { get; private set; } = HitboxShapeType.CIRCLE;
    public Vector2[] Points { get; private set; }
    public float Radius { get; private set; } = -1.0f;
    public float Length { get; private set; } = -1.0f;
    public float Scale { get; private set; } = 1.0f;
    public HitboxInteractionType InteractionType { get; private set; } = HitboxInteractionType.ALL;
    public HitboxRepeatPolicy RepeatPolicy { get; private set; } = HitboxRepeatPolicy.ONLY_ONCE;
    public float RepeatCooldown { get; private set; } = -1;
    public LayerMask LayerMask { get; private set; } = -1;
    public float Duration { get; private set; } = 0;
    public float Delay { get; private set; } = 0;
    public float StartRotationZ { get; private set; } = 0;
    public Vector2 Axis { get; private set; } = new Vector2(0, 0);
    public Dictionary<GameObject, float> InteractionTimeStamps { get; private set; } = null;
    public object CustomData { get; private set; } = null;
    public bool IsDelayed()
    {
        return Delay > 0;
    }
    public void TickDelay(float deltaTime)
    { 
        if(Delay < 0)
        {
            return;
        }
        Delay -= deltaTime;
    }

    public bool TickDuration(float deltaTime)
    {
        Duration -= deltaTime;
        return Duration > 0;
    }

    public class HitboxDataBuilder
    {
        private HitboxData data;
        public HitboxDataBuilder(HitboxData d)
        {
            data = d;
        }

        public HitboxDataBuilder ShapeType(HitboxShapeType shape)
        {
            data.ShapeType = shape;
            return this;
        }

        public HitboxDataBuilder InteractionType(HitboxInteractionType type)
        {
            data.InteractionType = type;
            return this;
        }

        public HitboxDataBuilder RepeatPolicy(HitboxRepeatPolicy policy)
        {
            data.RepeatPolicy = policy;
            return this;
        }

        public HitboxDataBuilder RepeatCooldown(float f)
        {
            data.RepeatCooldown = f;
            return this;
        }

        public HitboxDataBuilder Points(Vector2[] v)
        {
            data.Points = v;
            return this;
        }

        public HitboxDataBuilder Layer(LayerMask layer)
        {
            data.LayerMask = layer;
            return this;
        }

        public HitboxDataBuilder Radius(float r)
        {
            data.Radius = r;
            return this;
        }

        public HitboxDataBuilder Length(float l)
        {
            data.Length = l;
            return this;
        }

        public HitboxDataBuilder Scale(float s)
        {
            data.Scale = s;
            return this;
        }

        public HitboxDataBuilder Callback(Action<Collider2D, Hitbox> c)
        {
            data.OnCollision = c;
            return this;
        }

        public HitboxDataBuilder Discriminator(Func<Collider2D, bool> d)
        {
            data.Discriminator = d;
            return this;
        }

        public HitboxDataBuilder StartPosition(Vector3 v)
        {
            data.StartPosition = v;
            return this;
        }

        public HitboxDataBuilder Delay(float f)
        {
            data.Delay = f;
            return this;
        }

        public HitboxDataBuilder Duration(float f)
        {
            data.Duration = f;
            return this;
        }

        public HitboxDataBuilder CustomData(object o)
        {
            data.CustomData = o;
            return this;
        }

        public HitboxDataBuilder StartRotationZ(float f)
        {
            data.StartRotationZ = f;
            return this;
        }

        public HitboxDataBuilder Axis(Vector2 v)
        {
            data.Axis = v.normalized;
            return this;
        }

        public HitboxDataBuilder RotationInfo(float rotZ, Vector2 axis)
        {
            data.StartRotationZ = rotZ;
            data.Axis = axis.normalized;
            return this;
        }

        public HitboxDataBuilder InteractionTimeStamps(Dictionary<GameObject, float> d)
        {
            data.InteractionTimeStamps = d;
            return this;
        }

        public HitboxDataBuilder UpdateCallback(Action<Hitbox> callback)
        {
            data.UpdateCallback = callback;
            return this;
        }

        private bool Validate()
        {
            if (data.OnCollision == null)
            {
                Debug.LogError("ERROR: INVALID HURTBOX DATA NO COLLISION METHOD");
                return false;
            }
            if (data.StartPosition == Vector3.positiveInfinity)
            {
                Debug.LogError("ERROR: INVALID HURTBOX DATA NO START POSITION");
                return false;
            }
            if (data.RepeatPolicy == HitboxRepeatPolicy.COOLDOWN && data.RepeatCooldown == -1)
            {
                Debug.LogError("ERROR: INVALID HURTBOX DATA NO REPEAT COOLDOWN");
                return false;
            }
            if (data.ShapeType == HitboxShapeType.POLYGON && data.Points == null)
            {
                Debug.LogError("ERROR: POLYGON WITHOUT POINTS SPECIFIED");
                return false;
            }
            if ((data.ShapeType == HitboxShapeType.CIRCLE || data.ShapeType == HitboxShapeType.SQUARE || data.ShapeType == HitboxShapeType.PROJECTED_RECT) && data.Radius <= 0)
            {
                Debug.LogError("ERROR: SHAPE WITH INVALID RADIUS");
                return false;
            }
            if (data.ShapeType == HitboxShapeType.PROJECTED_RECT && data.Length <= 0)
            {
                Debug.LogError("ERROR: SQUARE WITH INVALID LENGTH");
                return false;
            }
            if (data.Duration <= float.Epsilon)
            {
                Debug.LogError("ERROR: HITBOX WITH TOO SHORT DURATION");
                return false;
            }
            return true;
        }

        public HitboxData Finalize()
        {
            if (!Validate())
            {
                throw new System.Exception("ERROR: INVALID HURTBOX DATA CREATED");
            }
            if(data.LayerMask == -1 || data.LayerMask == 0)
            {
                data.LayerMask = HitboxManager.instance.defaultLayer;
            }
            data.Validated = true;
            return data;
        }
    }

    public static HitboxDataBuilder GetBuilder()
    {
        return new HitboxDataBuilder(new HitboxData());
    }

    public static HitboxDataBuilder GetBuilder(HitboxDataAsset dataAsset)
    {
        return new HitboxDataBuilder(new HitboxData())
                .ShapeType(dataAsset.ShapeAsset.Type)
                .Points(dataAsset.ShapeAsset.Points)
                .Radius(dataAsset.ShapeAsset.Radius)
                .Length(dataAsset.ShapeAsset.Length)
                .InteractionType(dataAsset.InteractionType)
                .RepeatPolicy(dataAsset.RepeatPolicy)
                .RepeatCooldown(dataAsset.RepeatCooldown)
                .Layer(dataAsset.LayerMask)
                .Duration(dataAsset.Duration);
    }
}