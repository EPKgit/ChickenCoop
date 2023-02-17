using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    public class CollisionCallbackData
    {
        public uint ID;
        public Action<Collider2D> callback = null;
        public Func<Collider2D, bool> discriminator = null;
        public float range = -1;
    }

    private Collider2D playerMovementCollision;
    private List<CollisionCallbackData> callbacks;
    private float cachedMaxDistance;

    public void Awake()
    {
        callbacks = new List<CollisionCallbackData>();
        cachedMaxDistance = 0;
    }

    private static uint IDCounter = 0;
    public uint AddCallback(Action<Collider2D> callback, float range, Func<Collider2D, bool> discriminator)
    {
        if(callback == null)
        {
            return uint.MaxValue;
        }
        CollisionCallbackData data = new CollisionCallbackData() ;
        data.callback = callback;
        data.range = range;
        data.discriminator = discriminator;
        data.ID = IDCounter++;
        if(IDCounter == uint.MaxValue)
        {
            IDCounter = 0;
        }
        callbacks.Add(data);
        cachedMaxDistance = Mathf.Max(cachedMaxDistance, data.range);
        return data.ID;
    }

    public bool RemoveCallback(uint ID)
    {
        for(int x = 0; x < callbacks.Count; ++x)
        {
            if(callbacks[x].ID == ID)
            {
                float removedRange = callbacks[x].range;
                callbacks.RemoveAt(x);
                if (cachedMaxDistance == removedRange)
                {
                    cachedMaxDistance = -1;
                    foreach (var c in callbacks)
                    {
                        cachedMaxDistance = Mathf.Max(c.range);
                    }
                }
                return true;
            }
        }
        return false;
    }


    private void FixedUpdate()
    {
        if(callbacks.Count == 0)
        {
            return;
        }
        Collider2D[] results = Physics2D.OverlapCircleAll((Vector2)transform.position, cachedMaxDistance);
        foreach(var collider in results)
        {
            foreach(var data in callbacks)
            {
                if (data.range != -1)
                {
                    if (((Vector2)transform.position - collider.ClosestPoint(transform.position)).magnitude > data.range)
                    {
                        continue;
                    }
                }
                if(data.discriminator != null)
                {
                    if(!data.discriminator(collider))
                    {
                        continue;
                    }
                }
                data.callback(collider);
            }
        }
    }


    void OnDrawGizmos()
    {
        if(callbacks == null || callbacks.Count == 0)
        {
            return;
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, cachedMaxDistance);
        Gizmos.color = Color.red;
        foreach (var data in callbacks)
        {
            Gizmos.DrawWireSphere(transform.position, data.range);
        }
    }
}
