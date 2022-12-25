using System;
using System.Collections.Generic;
using UnityEngine;

public class Poolable : MonoBehaviour
{
    public event Action<GameObject> onPoolableReturned = delegate { };
    
    [HideInInspector]
	public GameObject prefab;

	/// <summary>
	/// Called when the poolable object is first created by the pool manager, get references that
	/// will be used multiple times over the objects lifetime
	/// </summary>
	public virtual void PoolInit(GameObject g)
	{
		prefab = g;
	}

	/// <summary>
	/// Called when the poolable object is fetched from the PoolManager. Implemented
	/// to reset the object back to it's prefab state.
	/// </summary>
	public virtual void Reset()
	{

	}

	/// <summary>
	/// Call instead of destroy on poolable objects, this will return the object to the pool to be reused.
	/// </summary>
	public virtual void DestroySelf()
	{
        onPoolableReturned(gameObject);
		if(prefab == null)
		{
            Destroy(gameObject);
            return;
        }
        PoolManager.instance.ReturnObject(prefab, gameObject);
	}
}
