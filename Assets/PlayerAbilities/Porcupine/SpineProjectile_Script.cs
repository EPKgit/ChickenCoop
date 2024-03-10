using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpineProjectile_Script : BaseLineTargeted
{
    public float damage;
    
    private List<IDamagable> alreadyHit = new List<IDamagable>();

    public override void Reset()
    {
        base.Reset();
        alreadyHit.Clear();
    }

    public void Setup(Vector3 pos, Vector3 direction, GameObject p, float d, float l = 1.0f)
    {
        transform.position = pos;
        rb.velocity = direction;
        creator = p;
        damage = d;
        timeLeftMax = timeLeftCurrent = l;
        transform.rotation = Quaternion.FromToRotation(Vector2.up, direction);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        IDamagable damagable = Lib.FindUpwardsInTree<IDamagable>(col.gameObject);
        if (damagable != null && !alreadyHit.Contains(damagable))
        {
            damagable.Damage
            (
                HealthChangeData.GetBuilder()
                    .Damage(damage)
                    .LocalSource(gameObject)
                    .OverallSource(creator)
                    .KnockbackData(KnockbackPreset.TINY)
                    .Target(damagable)
                    .Finalize()
            );
            alreadyHit.Add(damagable);

        }

        if (col.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            DestroySelf();
        }
    }

    protected override void Update()
    {
        base.Update();
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }
}
