using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using EnemyBehaviourSyntax;

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
        updateList.Add(new EnemyBehaviourAction().If(IsDisabled).Then(StopMovement).AndEnd());
        updateList.Add(new EnemyBehaviourAction().If(HasNoValidTarget).Then(StopMovement).Else(UpdateShieldAngle));
        updateList.Add(new EnemyBehaviourAction().If(IsKnockedBack).Then(KnockbackUpdate).AndEnd());
        updateList.Add(new EnemyBehaviourAction().Do(AnimationUpdate));
        updateList.Add(new EnemyBehaviourAction().If(CanMove).Then(Move).Else(StopMovement).AndEnd());

        movementSprings.Add(new MoveTowardsTargetPlayerSpring());
        movementSprings.Add(new SeperateEnemiesSpring());
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
		float angle = Vector3.Angle(arc.transform.up, mhced.data.BilateralData.LocalSource.transform.position - mhced.data.BilateralData.Target.transform.position);
        DebugFlags.Log(DebugFlags.Flags.ENEMYHEALTH, "" + arc.transform.up);
        DebugFlags.Log(DebugFlags.Flags.ENEMYHEALTH, "" + (mhced.data.BilateralData.LocalSource.transform.position - mhced.data.BilateralData.Target.transform.position));
        DebugFlags.Log(DebugFlags.Flags.ENEMYHEALTH, "" + angle);
		if(angle < blockAngle)
		{
            DebugFlags.Log(DebugFlags.Flags.ENEMYHEALTH, "cancelling");
			mhced.cancelled = true;
			ShieldClankEffect(mhced.data.BilateralData.LocalSource);
		}
        else
        {
			DamageEffect(mhced.data.BilateralData.LocalSource);
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
    }

    void UpdateShieldAngle()
    {
        arc.transform.rotation = Quaternion.Lerp(arc.transform.rotation, Quaternion.AngleAxis(Mathf.Rad2Deg * -Mathf.Atan2(rb.velocity.x, rb.velocity.y), Vector3.forward), turnSpeed);
    }
}
