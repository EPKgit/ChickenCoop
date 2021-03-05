using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseProjectile : Poolable
{
	protected float timeLeft;
	protected Rigidbody2D rb;
	protected GameObject creator;
  private SpriteRenderer[] renderers;

  void Awake() {
    renderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
  }
	public override void PoolInit(GameObject g)
	{
		base.PoolInit(g);
		rb = GetComponent<Rigidbody2D>();
	}
  public void Resize(Vector3 value) {
    foreach (SpriteRenderer sr in renderers)
    {
        sr.gameObject.transform.localScale = value;
    }
  }

	public override void Reset()
	{
		timeLeft = 6f;
	}

	public virtual void Setup(Vector3 pos, Vector3 direction, GameObject p)
	{
		transform.position = pos;
		rb.velocity = direction;
		creator = p;
    Resize(new Vector3(1,1,1));
	}

	protected virtual void Update()
	{
		timeLeft -= Time.deltaTime;
		if(timeLeft <= 0)
		{
			DestroySelf();
		}
	}
}
