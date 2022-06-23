using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    public static T Instance
    {
        get
        {
            return instance;
        }
    }
    public static T instance
    {
        get
        {
            if (_instance == null)
            {
                if (SingletonHelpers.managerObject == null)
                {
                    SingletonHelpers.managerObject = new GameObject();
                    DontDestroyOnLoad(SingletonHelpers.managerObject);
                    SingletonHelpers.managerObject.name = "Managers";
                }
                _instance = SingletonHelpers.managerObject.AddComponent(typeof(T)) as T;
                _instance.OnCreation();
            }
            return _instance;
        }
    }
    private static T _instance;

    protected virtual void Awake()
    {
        OverwriteSingleton((T)this);
    }
    protected virtual void OnCreation() { }

    protected void OverwriteSingleton(T inst)
    {
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        _instance = inst;
        OnCreation();
    }
}

public class Singleton<T> : ISingletonUpdate where T : new()
{
    public static T Instance
    {
        get
        {
            return instance;
        }
    }
    public static T instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T();
            }
            return _instance;
        }
    }
    private static T _instance;
    protected virtual void Awake() { }

    public void Update(float dt) 
    { 
        InternalUpdate(dt);
    }

    protected virtual void InternalUpdate(float dt) { }
    protected Singleton()
    {
        Awake();
    }
}

public interface ISingletonUpdate
{
    public abstract void Update(float dt);
}

public static class SingletonHelpers
{
    public static GameObject managerObject = null;
}