using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum HitboxShape
{
    CIRCLE,
    SQUARE,
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
[Serializable, CreateAssetMenu(menuName = "HitboxData")]
public class HitboxData : ScriptableObject
{ 
    //public non-serialized properties
    public bool valid { get; private set; } = false;
    public Vector3 StartPosition { get; private set; }
    public Action<Collider2D> OnCollision { get; private set; }
    public Func<Collider2D, bool> Discriminator { get; private set; }

    //public serialized accesor properties
    public HitboxShape Shape { get => shape; private set => shape = value; }
    public HitboxInteractionType InteractionType { get => interactionType; private set => interactionType = value; }
    public HitboxRepeatPolicy RepeatPolicy { get => repeatPolicy; private set => repeatPolicy = value; }
    public float RepeatCooldown { get => repeatCooldown; private set => repeatCooldown = value; }
    public LayerMask LayerMask { get => layerMask; private set => layerMask = value; }
    public float Radius { get => radius; private set => radius = value; }
    public float Duration { get => duration; private set => duration = value; }

    //private serialized backing fields
    [SerializeField] private HitboxShape shape = HitboxShape.CIRCLE;
    [SerializeField] private HitboxInteractionType interactionType = HitboxInteractionType.ALL;
    [SerializeField] private HitboxRepeatPolicy repeatPolicy = HitboxRepeatPolicy.ONLY_ONCE;
    [SerializeField] private float repeatCooldown = -1;
    [SerializeField] private LayerMask layerMask = 0;
    [SerializeField] private float radius = 0.5f;
    [SerializeField] private float duration = -1;

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
            if (data.Duration < 0)
            {
                return false;
            }
            if (data.RepeatPolicy == HitboxRepeatPolicy.COOLDOWN && data.RepeatCooldown == -1)
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
            if(data.layerMask == 0)
            {
                data.layerMask = HitboxManager.instance.defaultLayer;
            }
            data.valid = true;
            return data;
        }
    }

    public static HitboxDataBuilder GetBuilder()
    {
        return new HitboxDataBuilder(ScriptableObject.CreateInstance<HitboxData>());
    }
}