using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum HitboxShape
{
    CIRCLE,
    SQUARE,
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
    public Action<Collider2D> OnCollision { get; private set; }
    public Func<Collider2D, bool> Discriminator { get; private set; }


    //These have defaults and aren't necessary
    public HitboxShape Shape { get; private set; } = HitboxShape.CIRCLE;
    public Vector2[] Points { get; private set; }
    public HitboxInteractionType InteractionType { get; private set; } = HitboxInteractionType.ALL;
    public HitboxRepeatPolicy RepeatPolicy { get; private set; } = HitboxRepeatPolicy.ONLY_ONCE;
    public float RepeatCooldown { get; private set; } = -1;
    public LayerMask LayerMask { get; private set; } = -1;
    public float Radius { get; private set; } = 0.5f;
    public float Duration { get; private set; } = 0;
    public float StartRotationZ { get; private set; } = 0;
    public Dictionary<GameObject, float> InteractionTimeStamps { get; private set; } = null;

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

        public HitboxDataBuilder Shape(HitboxShape shape)
        {
            data.Shape = shape;
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

        public HitboxDataBuilder Callback(Action<Collider2D> c)
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

        public HitboxDataBuilder Duration(float f)
        {
            data.Duration = f;
            return this;
        }

        public HitboxDataBuilder StartRotationZ(float f)
        {
            data.StartRotationZ = f;
            return this;
        }

        public HitboxDataBuilder InteractionTimeStamps(Dictionary<GameObject, float> d)
        {
            data.InteractionTimeStamps = d;
            return this;
        }

        private bool Valid()
        {
            if (data.OnCollision == null)
            {
                return false;
            }
            if (data.StartPosition == Vector3.positiveInfinity)
            {
                return false;
            }
            if (data.RepeatPolicy == HitboxRepeatPolicy.COOLDOWN && data.RepeatCooldown == -1)
            {
                return false;
            }
            if (data.Shape == HitboxShape.POLYGON && data.Points == null)
            {
                return false;
            }
            return true;
        }

        public HitboxData Finalize()
        {
            if (!Valid())
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
                .Shape(dataAsset.Shape)
                .Points(dataAsset.Points)
                .InteractionType(dataAsset.InteractionType)
                .RepeatPolicy(dataAsset.RepeatPolicy)
                .RepeatCooldown(dataAsset.RepeatCooldown)
                .Layer(dataAsset.LayerMask)
                .Radius(dataAsset.Radius)
                .Duration(dataAsset.Duration);
    }
}