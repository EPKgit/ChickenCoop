using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxManager : MonoSingleton<HitboxManager>
{
    public GameObject hitboxPrefab;
    public LayerMask defaultLayer;

    private List<Hitbox> activeHitboxes;
    private List<(HitboxChainHandle, HitboxChain)> activeChains;

    protected override void OnCreation()
    {
        base.OnCreation();
        activeHitboxes = new List<Hitbox>();
        activeChains = new List<(HitboxChainHandle, HitboxChain)>();
    }
    public void SpawnHitbox(HitboxData data)
    {
        if(!data.Validated)
        {
            throw new System.Exception("Error: Invalid hitbox data passed to hitbox manager");
        }
        GameObject g = Instantiate(hitboxPrefab, data.StartPosition, Quaternion.Euler(0, 0, data.StartRotationZ));
        Hitbox hitbox = g.GetComponent<Hitbox>();
        hitbox.Setup(data);
        activeHitboxes.Add(hitbox);
    }

    public void SpawnHitbox(HitboxDataAsset dataAsset, Vector3 startPosition, Action<Collider2D, Hitbox> callback, float startRotationZ = 0)
    {
        SpawnHitbox(HitboxData.GetBuilder()
                .StartPosition(startPosition)
                .Callback(callback)
                .StartRotationZ(startRotationZ)
                .ShapeType(dataAsset.ShapeAsset.Type)
                .Points(dataAsset.ShapeAsset.Points)
                .Radius(dataAsset.ShapeAsset.Radius)
                .InteractionType(dataAsset.InteractionType)
                .RepeatPolicy(dataAsset.RepeatPolicy)
                .RepeatCooldown(dataAsset.RepeatCooldown)
                .Layer(dataAsset.LayerMask)
                .Duration(dataAsset.Duration)
                .Finalize());
    }

    public void SpawnHitbox(HitboxDataAsset dataAsset, Vector3 startPosition, Action<Collider2D, Hitbox> callback, float startRotationZ = 0, float durationOverride = 0)
    {
        SpawnHitbox(HitboxData.GetBuilder()
                .StartPosition(startPosition)
                .Callback(callback)
                .StartRotationZ(startRotationZ)
                .ShapeType(dataAsset.ShapeAsset.Type)
                .Points(dataAsset.ShapeAsset.Points)
                .Radius(dataAsset.ShapeAsset.Radius)
                .InteractionType(dataAsset.InteractionType)
                .RepeatPolicy(dataAsset.RepeatPolicy)
                .RepeatCooldown(dataAsset.RepeatCooldown)
                .Layer(dataAsset.LayerMask)
                .Duration(durationOverride)
                .Finalize());
    }

    private static RollingIDNumber IDCounter = new RollingIDNumber();
    public HitboxChainHandle StartHitboxChain(HitboxChainAsset data, Func<Vector2> positionCallback, Func<float> rotationCallback, Action<Collider2D, Hitbox, int> onHitCallback, Action<int> onHitboxSpawnCallback = null)
    {
        var handle = new HitboxChainHandle(IDCounter++);
        activeChains.Add((handle, new HitboxChain(ScriptableObject.Instantiate(data), positionCallback, rotationCallback, onHitCallback, onHitboxSpawnCallback)));
        return handle;
    }

    public void CancelHitboxChain(HitboxChainHandle handle)
    {
        for(int x = activeChains.Count - 1; x >= 0; --x)
        {
            if(activeChains[x].Item1 == handle)
            {
                activeChains.RemoveAt(x);
            }
        }
    }

    protected void Update()
    {
        TickChains();
        TickHitboxes();
    }

    void TickChains()
    {
        for(int x = activeChains.Count - 1; x >= 0; --x)
        {
            if (!activeChains[x].Item2.UpdateChain())
            {
                activeChains[x].Item1.SetCompleted();
                activeChains.RemoveAt(x);
            }
        }
    }

    void TickHitboxes()
    {
        for (int x = activeHitboxes.Count - 1; x >= 0; --x)
        {
            Hitbox hb = activeHitboxes[x];
            if (!hb.UpdateHitbox())
            {
                activeHitboxes.RemoveAt(x);
                hb.SetActive(false);
                hb.DestroySelf();
            }
        }
    }

    public static class Discriminators
    {
        public static bool Damagables(Collider2D col)
        {
            return col.GetComponent<IDamagable>() != null;
        }
        public static bool Healables(Collider2D col)
        {
            return col.GetComponent<IHealable>() != null;
        }
    }
}

public class HitboxChainHandle : IHandleWithIDNumber
{
    public bool Completed { get; private set; } = false;

    public void SetCompleted()
    {
        Completed = true;
    }

    public HitboxChainHandle(RollingIDNumber i) : base(i) { }

    public static implicit operator uint(HitboxChainHandle hch)
    {
        return hch.ID;
    }
}
