using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : BaseLineTargeted
{
	public GameObject bulletEffect;
	public float damage;

	public override void Reset()
	{
		base.Reset();
		GetComponentInChildren<TrailRenderer>()?.Clear();
	}

	public void Setup(Vector3 pos, Vector3 direction, GameObject p, float d, float l = 6.0f)
	{
		transform.position = pos;
		rb.velocity = direction;
		creator = p;
		damage = d;
        timeLeftMax = l;
	}

	void OnTriggerEnter2D(Collider2D col)
	{
        IDamagable damagable = Lib.FindUpwardsInTree<IDamagable>(col.gameObject);
        if(damagable != null)
        {
            DebugFlags.Log(DebugFlags.FLAGS.COLLISIONS, "trigger");
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
            DestroySelf();

        }
        if (col.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            DestroySelf();
        }
    }

	void BulletEffect(Vector3 position)
    {
		Quaternion rot = Quaternion.LookRotation(-GetComponent<Rigidbody2D>().velocity);
		GameObject effect = Instantiate(bulletEffect, position, rot);
		Destroy(effect, 1f);
	}

    protected override void Update()
    {
        base.Update();
        transform.Rotate(0, 0, Time.deltaTime * 1080);
    }
}
