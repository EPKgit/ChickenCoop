using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolData
{
	public GameObject defaultGO;
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

public class PoolManager : Singleton<PoolManager>
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
		if(DEBUGFLAGS.POOLMANAGER) Debug.Log("Requesting " + g.name);
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
				ret = Instantiate(g);
				ret.GetComponent<Poolable>().PoolInit(g);
				if(DEBUGFLAGS.POOLMANAGER) Debug.Log("POOL EMPTY, CREATING NEW");
			}
			else
			{
				ret = pools[g].pool[0];
				pools[g].pool.RemoveAt(0);
				if(DEBUGFLAGS.POOLMANAGER) Debug.Log("GOT FROM POOL");
			}
		}
		else
		{
			pools.Add(g, new PoolData(g));
			abnormalPools.Add(pools[g]);
			ret = Instantiate(g);
			ret.GetComponent<Poolable>().PoolInit(g);
			if(DEBUGFLAGS.POOLMANAGER) Debug.Log("POOL DIDN't EXIST");
		}
		ret.SetActive(true);
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
		if(DEBUGFLAGS.POOLMANAGER) Debug.Log("Returning " + spawned.name + " into pool " + prefab.name);
		if(pools.ContainsKey(prefab))
		{
			if(DEBUGFLAGS.POOLMANAGER) Debug.Log("Successful return");
			--pools[prefab].loans;
			pools[prefab].pool.Add(spawned);
			pools[prefab].currentTTL = pools[prefab].maxTTL;
			spawned.SetActive(false);
			return true;
		}
		if(DEBUGFLAGS.POOLMANAGER) Debug.Log("Failed return");
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
			pools[g].desiredSize = size;
			if(init)
			{
				NormalizePool(g);
			}
			return true;
		}
		pools.Add(g, new PoolData(g));
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
	/// <param name="size">The size of the pool</param>
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
		if(DEBUGFLAGS.POOLMANAGER) Debug.Log("Normalizing " + g.name);
		PoolData p = pools[g];
		int currentCount = p.pool.Count + p.loans;
		while(p.pool.Count < p.desiredSize)
		{
			GameObject temp = Instantiate(g);
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
