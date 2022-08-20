using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Bomb : BaseArcingProjectile
{
    public float damage;
    public float explosionRadius;

    public GameObject explosionPrefab;

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
            IDamagable damagable = Lib.FindUpwardsInTree<IDamagable>(col.gameObject);
            if(damagable !=null)
            {
                TargetingController controller = Lib.FindUpwardsInTree<TargetingController>(col.gameObject);
                if(controller?.TargetAffiliation != affiliation)
                {
                    damagable.Damage
                    (
                        HealthChangeData.GetBuilder()
                            .Damage(damage)
                            .LocalSource(gameObject)
                            .OverallSource(creator)
                            .KnockbackData(KnockbackPreset.BIG)
                            .Target(damagable)
                            .Finalize()
                    );
                }
            }
        }
        GameObject g = PoolManager.instance.RequestObject(explosionPrefab);
        g.transform.position = transform.position;
    }

    protected override void Update()
    {
        base.Update();
        transform.rotation = Quaternion.Euler(0, 0, (Time.frameCount % 10 < 5 ? 1 : -1 * 15));
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
