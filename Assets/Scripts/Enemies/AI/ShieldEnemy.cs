using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ShieldEnemy : BaseEnemy
{
    public override EnemyType type => EnemyType.SHIELD;

    public GameObject splashPrefab;
    public Gradient redSplashGradient;
    public Gradient whiteSplashGradient;
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

	void CheckIfBlocked(MutableHealthChangeEventData mhced)
	{
		float angle = Vector3.Angle(arc.transform.up, mhced.data.localSource.transform.position - mhced.data.target.transform.position);
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.ENEMYHEALTH, "" + arc.transform.up);
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.ENEMYHEALTH, "" + (mhced.data.localSource.transform.position - mhced.data.target.transform.position));
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.ENEMYHEALTH, "" + angle);
		if(angle < blockAngle)
		{
            DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.ENEMYHEALTH, "cancelling");
			mhced.cancelled = true;
			ShieldClankEffect(mhced.data.localSource);
		}
        else
        {
			DamageEffect(mhced.data.localSource);
    	}
	}

	private void ShieldClankEffect(GameObject source)
    {
        GameObject g = PoolManager.instance.RequestObject(splashPrefab);
        g.transform.position = source.transform.position;
        g.GetComponent<VisualEffect>()?.SetGradient(Shader.PropertyToID("ColorOverLife"), whiteSplashGradient);
	}

	private void DamageEffect(GameObject source)
    {
        GameObject g = PoolManager.instance.RequestObject(splashPrefab);
        g.transform.position = source.transform.position;
        g.GetComponent<VisualEffect>()?.SetGradient(Shader.PropertyToID("ColorOverLife"), redSplashGradient);
    }

    public override void SetEnemyEnabled(bool enabled)
    {
        base.SetEnemyEnabled(enabled);
        arc.SetActive(enabled);
        rb.velocity = Vector2.zero;

    }

    protected override bool Update()
	{
        if (!base.Update() || !CanMove())
        {
            rb.velocity = Vector2.zero;
            return false;
        }
        //Just walks towards the player
        Vector2 dir = (chosenPlayer.transform.position - transform.position).normalized;
        rb.velocity = dir * speed;
        arc.transform.rotation = Quaternion.Lerp(arc.transform.rotation, Quaternion.AngleAxis(Mathf.Rad2Deg * -Mathf.Atan2(dir.x, dir.y), Vector3.forward), turnSpeed);
        return true;
	}
}
