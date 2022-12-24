using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Shockwave : VFXPoolable
{
    private Action<IDamagable> onIntersectFunc;
    private Vector3 startPosition;
    private float lifetime;
    private float thickness;
    private float scale;
    private GameObject creator;
    private float startTime = float.MinValue;
    private List<TargetingController> alreadyEffected;
    private Targeting.Affiliation affiliation;
    private Vector3 intersection;


    public override void Reset()
	{
		base.Reset();
	}

    public void Setup(Vector3 startPosition, float lifetime, float thickness, float scale, GameObject creator, Action<IDamagable> toApplyCallback)
    {

        this.startPosition = transform.position = startPosition;
        this.lifetime = lifetime;
        this.thickness = thickness;
        this.scale = scale;
        transform.localScale = new Vector3(scale, scale, 1);
        this.creator = creator;
        startTime = Time.time;
        alreadyEffected = new List<TargetingController>();
        affiliation = Lib.FindDownwardsInTree<TargetingController>(creator).TargetAffiliation;
        onIntersectFunc = toApplyCallback;

        effect.SetFloat("MaxLifetime", lifetime);
        effect.SetFloat("Thickness", thickness);
        effect.SetFloat("Scale", scale);

        base.Reset();
    }

	protected override void Update()
	{
        base.Update();
		if(!active)
		{
            return;
        }
        float t = (Time.time - startTime) / lifetime;
        // var collisions = Physics2D.OverlapCircleAll((Vector2)transform.position, scale * t * 0.5f);
        var collisions = Physics2D.CircleCastAll((Vector2)transform.position, scale * t * 0.5f, Vector2.zero, 0.0f);
        foreach (var raycastHit in collisions)
        {
            var col = raycastHit.collider;
            var dist = (col.gameObject.transform.position - transform.position).magnitude;
            if(dist / (t * scale) < thickness) //not on the edge, we clamp the edge to the thickness
            {
                continue;
            }
            IDamagable damagable = Lib.FindUpwardsInTree<IDamagable>(col.gameObject);
            if (damagable != null)
            {
                TargetingController controller = Lib.FindUpwardsInTree<TargetingController>(col.gameObject);
                if (!alreadyEffected.Contains(controller) && controller?.TargetAffiliation != affiliation)
                {
                    alreadyEffected.Add(controller);
                    intersection = raycastHit.point;
                    onIntersectFunc(damagable);
                }
            }
        }
    }

    public Vector3 GetIntersectLocation()
    {
        return intersection;
    }

    void OnDrawGizmos()
    {
        if(!active || startTime == float.MinValue)
        {
            return;
        }
        if(lifetime <= 0)
        {
            return;
        }
        float t = (Time.time - startTime) / lifetime;
        Gizmos.DrawWireSphere(transform.position, scale * t * 0.5f);
    }
}
