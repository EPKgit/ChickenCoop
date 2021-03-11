using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    public static T instance
    {
        get
        {
            if (m_instance == null)
            {
                if (SingletonHelpers.managerObject == null)
                {
                    SingletonHelpers.managerObject = new GameObject();
                    DontDestroyOnLoad(SingletonHelpers.managerObject);
                    SingletonHelpers.managerObject.name = "Managers";
                }
                m_instance = SingletonHelpers.managerObject.AddComponent(typeof(T)) as T;
                m_instance.OnCreation();
            }
            return m_instance;
        }
    }
    private static T m_instance;

    protected virtual void Awake()
    {
        OverwriteSingleton((T)this);
    }
    protected virtual void OnCreation() { }

    protected void OverwriteSingleton(T inst)
    {
        if (m_instance != null)
        {
            Destroy(gameObject);
        }
        m_instance = inst;
        OnCreation();
    }
}

public class Singleton<T> where T : new()
{
    public static T instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new T();
            }
            return m_instance;
        }
    }
    private static T m_instance;
    protected virtual void Awake() { }
    protected Singleton()
    {
        Awake();
    }
}

public static class SingletonHelpers
{
    public static GameObject managerObject = null;
}