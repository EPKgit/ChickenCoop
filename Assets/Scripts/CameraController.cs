using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public Vector3 offset;

    private void Awake()
    {
        if(offset == Vector3.zero)
        {
            offset = transform.position - player.transform.position;
        }
    }

    private void LateUpdate()
    {
        transform.position = player.transform.position + offset;
    }
}
