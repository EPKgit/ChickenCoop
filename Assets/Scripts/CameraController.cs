using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public Vector2 minMaxZValue = new Vector2(-20.0f, -3.0f);
    public float zoomSpeed = 1.0f;

    private Vector3 localOffset;
    private float desiredZoom;

    private void Awake()
    {
        if(player == null)
        {
            this.enabled = false;
        }
        else
        {
            player.GetComponent<PlayerInput>()?.SetCamera(this);
        }
        localOffset = new Vector3(0, 0, transform.position.z);
        desiredZoom = transform.position.z;
    }

    public void OnCameraZoom(float value)
    {
        desiredZoom = Mathf.Clamp(desiredZoom + value * zoomSpeed, minMaxZValue.x, minMaxZValue.y);
    }

    void Update()
    {
        localOffset = Vector3.Lerp(localOffset, new Vector3(localOffset.x, localOffset.y, desiredZoom), Time.deltaTime);
    }

    private void LateUpdate()
    {
        transform.position = player.transform.position + localOffset;
    }
}
