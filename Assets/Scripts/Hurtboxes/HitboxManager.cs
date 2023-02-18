using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxManager : MonoSingleton<HitboxManager>
{
    public LayerMask defaultLayer;

    private List<Hitbox> activeHitboxes;

    protected override void OnCreation()
    {
        base.OnCreation();
        activeHitboxes = new List<Hitbox>();
    }
    public void SpawnHitbox(HitboxData data)
    {
        GameObject g = new GameObject();
        g.transform.position = data.StartPosition;
        Hitbox hitbox = g.AddComponent<Hitbox>();
        hitbox.Setup(data);
        activeHitboxes.Add(hitbox);
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
