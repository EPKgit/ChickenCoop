using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public Vector3 offset;

    private void Awake()
    {
        if(player == null)
        {
            this.enabled = false;
        }
    }

    private void LateUpdate()
    {
        transform.position = player.transform.position + offset;
    }
}
