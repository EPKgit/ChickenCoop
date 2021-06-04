using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolData
{
	public GameObject defaultGO;
    public GameObject transformParent;
	public List<GameObject> pool;
	public int desiredSize;
	public float maxTTL;
	public float currentTTL;
	public int loans;

	public PoolData(GameObject g, int i = 0, float f = 20)
	{
		defaultGO = g;
		pool = new List<GameObject>();
		desiredSize = i;
		maxTTL = f;
		loans = 0;
	}
}

public class PoolLoanToken
{
    private int amount;
    private GameObject objectToHold;
    public PoolLoanToken(GameObject g, int i, bool init = false)
    {
        objectToHold = g;
        amount = i;
        PoolManager.instance.AddPoolSize(objectToHold, amount, init);
    }
    ~PoolLoanToken()
    {
        PoolManager.instance.RemovePoolSize(objectToHold, amount);
    }
}

public class PoolManager : MonoSingleton<PoolManager>
{
    private Dictionary<GameObject, PoolData> pools;

	/// <summary>
	/// Represents the pools that are having issues that are going to be corrected
	/// by our update loop. Issues currently include not having the correct amount of 
	/// objects in the pool (either too many or too few).
	/// </summary>
	private List<PoolData> abnormalPools;

	protected override void Awake()
	{
		base.Awake();
		pools = new Dictionary<GameObject, PoolData>();
		abnormalPools = new List<PoolData>();
	}


	/// <summary>
	/// Polls our current misfit pools, and waits for their TTL to elapse. TTL is reset
	/// every time an object is requested or returned. If a long enough time has elapsed
	/// since the pool was used, we downsize down to our size.
	/// </summary>
	void Update()
	{	
		for(int x = abnormalPools.Count - 1; x >= 0; --x)
		{
			PoolData p = abnormalPools[x];
			p.currentTTL -= Time.deltaTime;
			if(p.currentTTL <= 0)
			{
				NormalizePool(p.defaultGO);
				abnormalPools.RemoveAt(x);
			}
		}
	}

	/// <summary>
	/// Requests an object from the PoolManager. If the object was not already being pooled, the
	/// object will be instantiated and registered for pooling. The object MUST be reinitialized
	/// with its Reset() method BY YOU, so all changes made in the course of its lifetime should be reset
	/// within that method.
	/// </summary>
	/// <param name="g">The gameobject that should be instantiated</param>
	/// <returns>The gameobject that is either created or fetched from a pool</returns>
	public GameObject RequestObject(GameObject g)
	{
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.POOLMANAGER, "Requesting " + g.name);
		GameObject ret;
		if(pools.ContainsKey(g))
		{
			if(pools[g].pool.Count == 0)
			{
				if(!abnormalPools.Contains(pools[g]))
				{
					abnormalPools.Add(pools[g]);
				}
				pools[g].currentTTL = pools[g].maxTTL;
				ret = Instantiate(g, pools[g].transformParent.transform);
				ret.GetComponent<Poolable>().PoolInit(g);
				DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.POOLMANAGER, "POOL EMPTY, CREATING NEW");
			}
			else
			{
				ret = pools[g].pool[0];
				pools[g].pool.RemoveAt(0);
				DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.POOLMANAGER, "GOT FROM POOL");
			}
		}
		else
		{
            pools.Add(g, new PoolData(g));
			abnormalPools.Add(pools[g]);
            pools[g].transformParent = new GameObject();
            pools[g].transformParent.name = "Pool for " + g.name;
            pools[g].transformParent.tag = "PoolHolder";
            ret = Instantiate(g, pools[g].transformParent.transform);
            ret.GetComponent<Poolable>().PoolInit(g);
			DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.POOLMANAGER, "POOL DIDN't EXIST");
		}
		ret.SetActive(true);
        ret.GetComponent<Poolable>().Reset();
		++pools[g].loans;
		return ret;
	}

	/// <summary>
	/// Puts the object back in the pool for use later.
	/// </summary>
	/// <param name="prefab">The gameobject prefab that the actual gameobject was spawned</param>
	/// <param name="spawned">The spawned gameobject to return</param>
	/// <returns>
	/// Returns true if the gameobject was succesfully reclaimed, false if the gameobject was never
	/// a part of a pool, which means it was never requested.
	/// </returns>
	public bool ReturnObject(GameObject prefab, GameObject spawned)
	{
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.POOLMANAGER, "Returning " + spawned.name + " into pool " + prefab.name);
		if(pools.ContainsKey(prefab))
		{
			DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.POOLMANAGER, "Successful return");
			--pools[prefab].loans;
			pools[prefab].pool.Add(spawned);
			pools[prefab].currentTTL = pools[prefab].maxTTL;
			spawned.SetActive(false);
			return true;
		}
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.POOLMANAGER, "Failed return");
		Destroy(spawned);
		return false;	
	}

	/// <summary>
	/// Same as return object, but waits a specified period before trying to reclaim
	/// the objects. This version cannot return a value. BE CAREFUL USING THIS. If the object
	/// is returned before this method returns it, it may be loaned out again and returned
	/// at an unexpected time.
	/// </summary>
	/// <param name="prefab">The gameobject prefab that the actual gameobject was spawned</param>
	/// <param name="spawned">The spawned gameobject to return</param>
	/// <param name="f">The amount of time to wait</param>
	public void ReturnObject(GameObject prefab, GameObject spawned, float f)
	{
		StartCoroutine(ReturnHelper(prefab, spawned, f));
	}

	private IEnumerator ReturnHelper(GameObject prefab, GameObject spawned, float f)
	{
		yield return new WaitForSeconds(f);
		ReturnObject(prefab, spawned);
	}

	/// <summary>
	/// Sets the desired amount of objects that can be loaned out at any one time by the pool.
	/// The PoolManager will attempt to keep this many objects within the pool at any given time,
	/// whether inactive or loaned out. If more are created than this, the PoolManager will destroy
	/// them after TTL, down to the size.
	/// </summary>
	/// <param name="g">The gameobject representing the pool</param>
	/// <param name="size">The size of the pool</param>
	/// <param name="init">If the pool should attempt to fix the pool size all in one frame</param>
	/// <returns>Returns true if the gameobject pool exists already, false if the pool had to be created</returns>
	public bool SetPoolSize(GameObject g, int size, bool init = false)
	{
		if(pools.ContainsKey(g))
		{
            if (size <= 0)
            {
                pools.Remove(g);
                return true;
            }
			pools[g].desiredSize = size;
			if(init)
			{
				NormalizePool(g);
			}
			return true;
		}
        if(size <= 0)
        {
            return false;
        }
		pools.Add(g, new PoolData(g));
        pools[g].transformParent = new GameObject();
        pools[g].transformParent.name = "Pool for " + g.name;
        pools[g].transformParent.tag = "PoolHolder";
        pools[g].desiredSize = size;
		if(init)
		{
			NormalizePool(g);
		}
		else
		{
			pools[g].currentTTL = 0;
			abnormalPools.Add(pools[g]);
		}
		return false;
	}

	/// <summary>
	/// Adds to the poolsize instead of reseting, can be useful if multiple entities are using the same pool
	/// potentially e.g. if multiple of the same ability can exist that uses the same projectile. This way
	/// you can reserve yourself some pool size.
	/// </summary>
	/// <param name="g">The gameobject representing the pool</param>
	/// <param name="size">The amount to add to the pool</param>
	/// <param name="init">If the pool should attempt to fix the pool size all in one frame</param>
	/// <returns>Returns true if the gameobject pool exists already, false if the pool had to be created</returns>
	public bool AddPoolSize(GameObject g, int size, bool init = false)
	{
		if(!pools.ContainsKey(g))
		{
			return SetPoolSize(g, size, init);
		}
		return SetPoolSize(g, pools[g].desiredSize + size, init);
	}

    /// <summary>
	/// Should be called to return the allocated pool entities that you requested. If the amount removed it greater than the amount
    /// allocated, the pool will be removed entirely
	/// </summary>
	/// <param name="g">The gameobject representing the pool</param>
	/// <param name="size">The amount to remove from the pool</param>
	/// <returns>Returns true if the gameobject pool exists already, false if the pool otherwise</returns>
	public bool RemovePoolSize(GameObject g, int size)
    {
        if (!pools.ContainsKey(g))
        {
            return false;
        }
        return SetPoolSize(g, pools[g].desiredSize - size, true);
    }

    /// <summary>
    /// Request a token that represents a certain amount of objects that are added into the pool for you. This token allocates tokens on creation and returns them on deletion, allowing for 
    /// RAII style resource management for pool request
    /// </summary>
    /// <param name="g">The gameobject representing the pool</param>
	/// <param name="size">The amount to add to the pool</param>
	/// <param name="init">If the pool should attempt to fix the pool size all in one frame</param>
    /// <returns>A token that will automatically cleanup the pool at some point after going out of scope</returns>
    public PoolLoanToken RequestLoan(GameObject g, int size, bool init)
    {
        return new PoolLoanToken(g, size, init);
    }

    /// <summary>
    /// Sets the desired amount of time that the PoolManager will wait before shrinking the size
    /// of the pool back down to the desiredSize.
    /// </summary>
    /// <param name="g">The gameobject representing the pool</param>
    /// <param name="f">The amount of time to wait in seconds</param>
    /// <returns>Returns true if the gameobject pool exists already, false if the pool had to be created</returns>
    public bool SetTTL(GameObject g, float f)
	{
		if(pools.ContainsKey(g))
		{
			pools[g].maxTTL = f;
			return true;
		}
		pools.Add(g, new PoolData(g));
        pools[g].transformParent = new GameObject();
        pools[g].transformParent.name = "Pool for " + g.name;
        pools[g].transformParent.tag = "PoolHolder";
        pools[g].maxTTL = f;
		pools[g].currentTTL = 0;
		return false;
	}

	/// <summary>
	/// Resets the pool to the desiredSize all in one frame
	/// </summary>
	/// <param name="g">The pool to normalize</param>
	private void NormalizePool(GameObject g)
	{
		if(!pools.ContainsKey(g) || pools[g].desiredSize <= 0)
		{
			return;
		}
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.POOLMANAGER, "Normalizing " + g.name);
		PoolData p = pools[g];
		int currentCount = p.pool.Count + p.loans;
		while(p.pool.Count < p.desiredSize)
		{
			GameObject temp = Instantiate(g, p.transformParent.transform);
			temp.name = g.name + p.pool.Count;
			temp.SetActive(false);
			temp.GetComponent<Poolable>().PoolInit(g);
			p.pool.Add(temp);
		}
		while(p.pool.Count > p.desiredSize)
		{
			Destroy(p.pool[0]);
			p.pool.RemoveAt(0);
		}
	}

    /// <summary>
    /// Changes the transform parent of all of the objects in a pool to a certain gameobject
    /// </summary>
    /// <param name="key">The key for the pool to change</param>
    /// <param name="newParent">The new transform parent</param>
    /// <returns>Returns true if succesful, false if the pool didn't exist</returns>
    public bool ChangeParentOfPool(GameObject key, Transform newParent)
    {
        if (!pools.ContainsKey(key))
        {
            return false;
        }
        PoolData pool = pools[key];
        foreach(GameObject g in pool.pool)
        {
            g.transform.SetParent(newParent, false);
        }
        if(pool.transformParent.tag == "PoolHolder")
        {
            Destroy(pool.transformParent);
        }
        pool.transformParent = newParent.gameObject;
        return true;
    }

	public List<PoolData> GetPools()
	{
		List<PoolData> data = new List<PoolData>();
		foreach(var l in pools.Values)
		{
			data.Add(l);
		}
		return data;
	}
}
