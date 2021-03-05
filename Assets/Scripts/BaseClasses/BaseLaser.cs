using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseLaser : Poolable
{
    public float tickTime;
    public LayerMask laserMask;
    public float segmentLength;
    public float totalLength;
    protected GameObject creator;
    protected Dictionary<BaseHealth, float> timers;
    protected List<BaseHealth> inside;
    protected GameObject copy;

    void Awake()
    {
        timers = new Dictionary<BaseHealth, float>();
        inside = new List<BaseHealth>();
    }
    public override void PoolInit(GameObject g)
    {
        base.PoolInit(g);
    }

    public virtual void Setup(Vector3 pos, Vector3 direction, GameObject p, float lengthLeft)
    {
        //transform.position = pos;
        creator = p;
        totalLength = lengthLeft;
        UpdateLine(pos, direction);
    }

    public virtual void UpdateLine(Vector3 pos, Vector3 direction)
    {
        GameObject.Destroy(copy);

        direction = Lib.DefaultDirectionCheck(direction);

        Vector3 endPoint = pos + direction * segmentLength;

        float distance = segmentLength;

        RaycastHit2D hit = Physics2D.Raycast(pos, direction, segmentLength, laserMask);

        if (hit)
        {
            endPoint = hit.point;
            distance = hit.distance;
            if (Lib.HasTagInHierarchy(hit.transform.gameObject, "Mirror"))
            {
                copy = GameObject.Instantiate(this.gameObject);
                copy.GetComponent<BaseLaser>().Setup(hit.point + Vector2.Reflect(direction, hit.normal) * .1f, Vector2.Reflect(direction, hit.normal), null, totalLength - distance);
            }
        }
        else if (totalLength > 1)
        {
            copy = GameObject.Instantiate(this.gameObject);
            copy.GetComponent<BaseLaser>().Setup(endPoint, direction, null, totalLength - distance);
        }

        Vector2 newpos = pos + direction * distance/2f;
        transform.position = new Vector3(newpos.x, newpos.y, .25f);

        float angle = Mathf.Atan2(endPoint.y - pos.y, endPoint.x - pos.x) * Mathf.Rad2Deg + 90;

        Vector3 r = new Vector3(0 , 0, angle);
        Quaternion q = new Quaternion();
        q.eulerAngles = r;
        transform.rotation = q;

        transform.localScale = new Vector3(1, distance, 1);
    }

    protected virtual void Update()
    {
        if (creator != null)
            UpdateLine(creator.transform.position, creator.GetComponent<PlayerInput>().GetAimDirection());

        foreach (BaseHealth i in inside)
        {
            if (timers.ContainsKey(i))
            {
                timers[i] -= Time.deltaTime;
                if (timers[i] <= 0)
                {
                    timers[i] = tickTime;

                    OnTick(i);
                }
            }
            else
            {
                timers.Add(i, tickTime);
            }
        }
    }

    protected virtual void OnTick(BaseHealth target)
    {
        if (Lib.HasTagInHierarchy(target.gameObject, "Player"))
            return;

        target.Damage(1, creator, target.gameObject);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        BaseHealth i = Lib.FindInHierarchy<BaseHealth>(col.gameObject);
        if (i == null)
        {
            return;
        }
        this.enabled = true;
        if (!inside.Contains(i))
        {
            inside.Add(i);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        BaseHealth i = Lib.FindInHierarchy<BaseHealth>(col.gameObject);
        if (i == null)
        {
            return;
        }
        inside.Remove(i);
    }

    void OnDestroy()
    {
        if (copy != null)
            GameObject.Destroy(copy);
    }
}
