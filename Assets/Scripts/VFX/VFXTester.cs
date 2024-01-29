using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXTester : MonoBehaviour
{
    public GameObject VFX;

    void Start()
    {
        PoolManager.instance.RequestObject(VFX).transform.position = transform.position;
    }
}
