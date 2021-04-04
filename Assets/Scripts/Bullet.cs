using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : BaseProjectile
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
		if(!Lib.HasTagInHierarchy(col.gameObject, "Enemy") && col.gameObject.layer!=13)
    {
			return;
		}
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.COLLISIONS, "trigger");
		Lib.FindInHierarchy<IDamagable>(col.gameObject)?.Damage(damage, gameObject, creator);
    	//BulletEffect(transform.position);
   		DestroySelf();
	}

	void BulletEffect(Vector3 position) {
		Quaternion rot = Quaternion.LookRotation(-GetComponent<Rigidbody2D>().velocity);
		GameObject effect = Instantiate(bulletEffect, position, rot);
		Destroy(effect, 1f);
	}
}
