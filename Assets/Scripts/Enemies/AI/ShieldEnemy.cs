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
    protected override void OnDisable()
	{
        base.OnDisable();
        hp.preDamageEvent -= CheckIfBlocked;
        PoolManager.instance?.RemovePoolSize(splashPrefab, 10);
    }

	void CheckIfBlocked(MutableHealthChangeEventData mhced)
	{
		float angle = Vector3.Angle(arc.transform.up, mhced.data.LocalSource.transform.position - mhced.data.Target.transform.position);
        DebugFlags.Log(DebugFlags.FLAGS.ENEMYHEALTH, "" + arc.transform.up);
        DebugFlags.Log(DebugFlags.FLAGS.ENEMYHEALTH, "" + (mhced.data.LocalSource.transform.position - mhced.data.Target.transform.position));
        DebugFlags.Log(DebugFlags.FLAGS.ENEMYHEALTH, "" + angle);
		if(angle < blockAngle)
		{
            DebugFlags.Log(DebugFlags.FLAGS.ENEMYHEALTH, "cancelling");
			mhced.cancelled = true;
			ShieldClankEffect(mhced.data.LocalSource);
		}
        else
        {
			DamageEffect(mhced.data.LocalSource);
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

    protected override void Update()
	{
        base.Update();
        if (!base.UpdateMovement() || !CanMove())
        {
            rb.velocity = Vector2.zero;
            return;
        }
        //Just walks towards the player
        Vector2 dir = (chosenPlayer.transform.position - transform.position).normalized;
        rb.velocity = dir * speed;
        arc.transform.rotation = Quaternion.Lerp(arc.transform.rotation, Quaternion.AngleAxis(Mathf.Rad2Deg * -Mathf.Atan2(dir.x, dir.y), Vector3.forward), turnSpeed);
	}
}
