using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ShieldEnemy : BaseEnemy
{
    public GameObject splashPrefab;
    public GameObject arc;
	public float blockAngle;
	public float turnSpeed = 0.05f;

    protected override void Awake()
    {
        base.Awake();
    }

    void OnEnable()
	{
		hp.preDamageEvent += CheckIfBlocked;
        PoolManager.instance.AddPoolSize(splashPrefab, 10, true);
    }
    void OnDisable()
	{
		hp.preDamageEvent -= CheckIfBlocked;
        PoolManager.instance.RemovePoolSize(splashPrefab, 10);
    }

	void CheckIfBlocked(HealthChangeEventData hced)
	{
		float angle = Vector3.Angle(arc.transform.up, hced.localSource.transform.position - hced.target.transform.position);
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.ENEMYHEALTH, "" + arc.transform.up);
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.ENEMYHEALTH, "" + (hced.localSource.transform.position - hced.target.transform.position));
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.ENEMYHEALTH, "" + angle);
		if(angle < blockAngle)
		{
            DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.ENEMYHEALTH, "cancelling");
			hced.cancelled = true;
			ShieldClankEffect(hced.localSource);
		}
        else
        {
			DamageEffect(hced.localSource);
    	}
	}

	private void ShieldClankEffect(GameObject source)
    {
        GameObject g = PoolManager.instance.RequestObject(splashPrefab);
        g.transform.position = source.transform.position;
        g.GetComponent<VisualEffect>()?.SetBool(Shader.PropertyToID("Red"), false);
        Poolable p = g.GetComponent<Poolable>();
	}

	private void DamageEffect(GameObject source)
    {
        GameObject g = PoolManager.instance.RequestObject(splashPrefab);
        g.transform.position = source.transform.position;
        g.GetComponent<VisualEffect>()?.SetBool(Shader.PropertyToID("Red"), true);
        Poolable p = g.GetComponent<Poolable>();
    }

    protected override bool Update()
	{
        if (!base.Update())
        {
            return false;
        }
        //Just walks towards the player
        Vector2 dir = (chosenPlayer.transform.position - transform.position).normalized;
        rb.velocity = dir * speed;
        arc.transform.rotation = Quaternion.Lerp(arc.transform.rotation, Quaternion.AngleAxis(Mathf.Rad2Deg * -Mathf.Atan2(dir.x, dir.y), Vector3.forward), turnSpeed);
        return true;
	}
}
