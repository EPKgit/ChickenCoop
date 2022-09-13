using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarUnitComponent : MonoBehaviour
{
    public int range;
    public Vector3 position
    {
        get => transform.position;
    }

    void OnEnable()
    {
        FogOfWarManager.instance.RegisterUnit(this);
    }

    void OnDisable()
    {
        FogOfWarManager.instance?.DeregisterUnit(this);
    }
}
