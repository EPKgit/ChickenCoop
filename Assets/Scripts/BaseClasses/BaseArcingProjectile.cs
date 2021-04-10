using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseArcingProjectile : Poolable
{
    public float arcSteepness = 1;
    public float arcTime = 2.0f;

    protected GameObject creator;
    protected SpriteRenderer[] renderers;
    protected float timer;
    protected Vector3 startPosition;
    protected Vector3 endPosition;

    void Awake()
    {
        renderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
    }
    public override void PoolInit(GameObject g)
    {
        base.PoolInit(g);
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
        timer = 0;
    }

    public virtual void Setup(Vector3 startPosition, Vector3 endPosition, GameObject p)
    {
        transform.position = startPosition;
        this.startPosition = startPosition;
        this.endPosition = endPosition;
        creator = p;
        Resize(new Vector3(1, 1, 1));
    }

    protected virtual void Update()
    {
        timer += Time.deltaTime;
        if(timer > arcTime)
        {
            DestroySelf();
        }
        float t = timer / arcTime;
        Vector3 newPosition = Vector3.Lerp(startPosition, endPosition, t);
        t = 2 * t - 1;
        t *= t; //remap from 0-1 to the input to a quadratic
        newPosition.z = arcSteepness * (t) - arcSteepness;
        transform.position = newPosition;
    }
}
