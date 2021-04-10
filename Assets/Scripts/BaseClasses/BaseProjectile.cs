using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseLineTargeted : Poolable
{
	protected float timeLeftMax = 6.0f;
	protected float timeLeftCurrent;
	protected Rigidbody2D rb;
	protected GameObject creator;
    private SpriteRenderer[] renderers;

    void Awake()
    {
        renderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
    }
    public override void PoolInit(GameObject g)
	{
		base.PoolInit(g);
		rb = GetComponent<Rigidbody2D>();
	}
    public void Resize(Vector3 value)
    {
        foreach (SpriteRenderer sr in renderers)
        {
            sr.gameObject.transform.localScale = value;
        }
    }

    public override void Reset()
	{
        timeLeftCurrent = timeLeftMax;
	}

	public virtual void Setup(Vector3 pos, Vector3 direction, GameObject p)
	{
		transform.position = pos;
		rb.velocity = direction;
		creator = p;
        Resize(new Vector3(1, 1, 1));
	}

	protected virtual void Update()
	{
        timeLeftCurrent -= Time.deltaTime;
		if(timeLeftCurrent <= 0)
		{
			DestroySelf();
		}
	}
}
