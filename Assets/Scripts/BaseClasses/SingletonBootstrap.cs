using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonBootstrap : MonoSingleton<SingletonBootstrap>
{
    private List<ISingletonUpdate> singletons = new List<ISingletonUpdate>();

    void Update()
    {
        foreach(ISingletonUpdate s in singletons)
        {
            s.Update(Time.deltaTime);
        }
    }

    public void AddToUpdateList(ISingletonUpdate s)
    {
        singletons.Add(s);
    }

    public void RemoveFromUpdateList(ISingletonUpdate s)
    {
        singletons.Remove(s);
    }
}
