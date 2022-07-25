using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.InputSystem;

public class Ability_Blank : Ability
{
	private int? collisionHandle;
    public GameObject splashPrefab;

    public override string GetTooltip()
    {
        return string.Format(tooltipDescription, aoe, maxDuration);
    }

    public override void Initialize(PlayerAbilities pa)
    {
        base.Initialize(pa);
        PoolManager.instance.AddPoolSize(splashPrefab, 10, true);
    }

    public override void Cleanup(PlayerAbilities pa)
    {
        base.Cleanup(pa);
        PoolManager.instance.RemovePoolSize(splashPrefab, 10);
    }

    public override void Reinitialize()
	{
		base.Reinitialize();
        collisionHandle = null;
	}

    protected override void UseAbility()
	{
        base.UseAbility();
        collisionHandle = playerAbilities?.collision?.AddCallback(OnCollision, aoe, null);
    }

    public void OnCollision(Collider2D collider)
    {
        if(collider.gameObject.layer == LayerMask.NameToLayer("EnemyProjectiles"))
        {
            GameObject g = PoolManager.instance.RequestObject(splashPrefab);
            g.transform.position = collider.transform.position;

            Poolable p = collider.gameObject.GetComponent<Poolable>();
            if(p != null)
            {
                p.DestroySelf();
            }
            else
            {
                Destroy(collider.gameObject);
            }
        }
    }

	public override void FinishAbility()
	{
        if (collisionHandle.HasValue)
		{
            playerAbilities.collision.RemoveCallback(collisionHandle.Value);
            collisionHandle = null;
		}
        base.FinishAbility();
    }
}