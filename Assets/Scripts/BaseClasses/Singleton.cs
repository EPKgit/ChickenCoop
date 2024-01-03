using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMonoStub : MonoBehaviour
{
    protected virtual void Awake()
    {

    }
}

public abstract class MonoSingleton<T> : SingletonMonoStub where T : MonoSingleton<T>
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
                //due to execution order sometimes scripts will call singletons after they've been destroyed and we don't
                //want to respawn them so anytime any singleton goes out of scope we stop spawning new ones 
                if(SingletonHelpers.sceneUnloading)
                {
                    return null;
                }
                if (SingletonHelpers.managerObject == null)
                {
                    SingletonHelpers.managerObject = GameObject.Find(SingletonHelpers.managerObjectName);
                }
                if (SingletonHelpers.managerObject == null)
                {
                    SingletonHelpers.managerObject = new GameObject();
                    DontDestroyOnLoad(SingletonHelpers.managerObject);
                    SingletonHelpers.managerObject.name = SingletonHelpers.managerObjectName;
                }
                T temp = SingletonHelpers.managerObject.GetComponent<T>();
                if(temp != null)
                {
                    _instance = temp;
                }
                else
                {
                    _instance = SingletonHelpers.managerObject.AddComponent(typeof(T)) as T;
                }
            }
            return _instance;
        }
    }
    private static T _instance;


    /// <summary>
    /// DO NOT OVERRIDE IN BASE CLASSES, DO ALL INIT IN OnCreation
    /// </summary>
    protected override sealed void Awake()
    {
        OverwriteSingleton((T)this);
    }
    protected virtual void OnCreation() { }

    protected void OverwriteSingleton(T inst)
    {
        if (_instance != null)
        {
            if(_instance == this)
            {
                return;
            }
            Destroy(this);
        }
        _instance = inst;
        OnCreation();
    }

    /// <summary>
    /// This can get overriden in child classes if they don't want to kill all singletons when they die
    /// </summary>
    protected virtual void OnDisable()
    {
        SingletonHelpers.sceneUnloading = true;
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
    public static string managerObjectName = "Managers";
    public static bool sceneUnloading = false;
}