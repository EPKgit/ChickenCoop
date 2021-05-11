using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : BaseArcingProjectile
{
    public float damage;
    public float explosionRadius;

    public override void Reset()
    {
        base.Reset();
        GetComponentInChildren<TrailRenderer>()?.Clear();
    }

    protected override void OnEnd()
    {
        var collisions = Physics2D.OverlapCircleAll((Vector2)transform.position, explosionRadius);
        foreach(var col in collisions)
        {
            IDamagable damagable = Lib.FindInHierarchy<IDamagable>(col.gameObject);
            if(damagable !=null)
            {
                TargetingController controller = Lib.FindInHierarchy<TargetingController>(col.gameObject);
                if(controller?.TargetAffiliation != affiliation)
                {
                    damagable?.Damage(damage, gameObject, creator);
                }
            }
        }
    }


    protected override void Update()
    {
        base.Update();
        transform.rotation = Quaternion.Euler(0, 0, (Time.frameCount % 10 < 5 ? 1 : -1 * 15));
    }
}
