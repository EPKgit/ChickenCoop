using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : BaseArcingProjectile
{
    public float damage;

    public override void Reset()
    {
        base.Reset();
        GetComponentInChildren<TrailRenderer>()?.Clear();
    }


    void OnTriggerEnter2D(Collider2D col)
    {
        if (!Lib.HasTagInHierarchy(col.gameObject, "Enemy") && col.gameObject.layer != 13)
        {
            return;
        }
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.COLLISIONS, "trigger");
        Lib.FindInHierarchy<IDamagable>(col.gameObject)?.Damage(damage, gameObject, creator);
        DestroySelf();
    }
}
