using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseArcingProjectile : Poolable
{
    public Targeting.Affiliation affiliation = Targeting.Affiliation.NONE;
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

    public virtual void Setup(Vector3 startPosition, Vector3 endPosition, GameObject p, Targeting.Affiliation a)
    {
        transform.position = startPosition;
        this.startPosition = startPosition;
        this.endPosition = endPosition;
        creator = p;
        affiliation = a;
        Resize(new Vector3(1, 1, 1));
    }

    protected virtual void OnEnd()
    {

    }

    protected virtual void Update()
    {
        timer += Time.deltaTime;
        if(timer > arcTime)
        {
            OnEnd();
            DestroySelf();
        }
        transform.position = Lib.GetArcPosition(startPosition, endPosition, timer, arcTime, arcSteepness);
    }
}
