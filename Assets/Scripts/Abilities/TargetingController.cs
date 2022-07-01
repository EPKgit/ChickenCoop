using System.Collections;
using System.Collections.Generic;
using Targeting;
using UnityEngine;

public class TargetingController : MonoBehaviour, Targeting.ITargetable
{
    public Affiliation TargetAffiliation
    {
        get
        {
            return _targetAffiliation;
        }
        set
        {
            _targetAffiliation = value;
        }
    }
    [SerializeField]
    private Affiliation _targetAffiliation;

    public int Priority
    {
        get
        {
            return _priority;
        }
    }
    [SerializeField]
    private int _priority;


    public GameObject Attached
    {
        get
        {
            return gameObject;
        }
    }

    private new Collider2D collider;
    private LayerMask layer;

    void OnEnable()
    {
        layer = LayerMask.NameToLayer("Targeting");
        var cols = GetComponentsInChildren<Collider2D>();
        foreach(Collider2D c in cols)
        {
            if(c.gameObject.layer == layer)
            {
                collider = c;
                break;
            }
        }
        if(collider == null)
        {
            GameObject child = new GameObject();
            child.layer = layer;
            child.name = "TargetingCollider";
            collider = child.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            child.transform.SetParent(transform, true);
            child.transform.localPosition = Vector3.zero;
        }
    }

    public void SetTargetingEnabled(bool enabled)
    {
        collider.enabled = enabled;
    }
}
