using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxManager : MonoSingleton<HitboxManager>
{
    public GameObject hitboxPrefab;
    public LayerMask defaultLayer;

    private List<Hitbox> activeHitboxes;

    protected override void OnCreation()
    {
        base.OnCreation();
        activeHitboxes = new List<Hitbox>();
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

    public void SpawnHitbox(HitboxDataAsset data, Vector3 startPosition, Action<Collider2D> callback, float startRotationZ = 0)
    {
        SpawnHitbox(HitboxData.GetBuilder()
                .StartPosition(startPosition)
                .Callback(callback)
                .StartRotation(startRotationZ)
                .Shape(data.shape)
                .Points(data.points)
                .InteractionType(data.interactionType)
                .RepeatPolicy(data.repeatPolicy)
                .RepeatCooldown(data.repeatCooldown)
                .Layer(data.layerMask)
                .Radius(data.radius)
                .Duration(data.duration)
                .Finalize());
    }

    protected void Update()
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
