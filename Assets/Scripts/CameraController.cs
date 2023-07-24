using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public Vector2 minMaxZValue = new Vector2(-20.0f, -3.0f);
    public float zoomSensitivity = 1.0f;
    public float zoomLerpSpeed = 3.0f;

    private Vector3 localOffset;
    private float desiredZoom;

    private void Awake()
    {
        if(player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                throw new System.Exception("ERROR: NO PLAYER FOUND ON CAMERA");
            }
        }
        player.GetComponent<PlayerInput>()?.SetCamera(this);
        localOffset = new Vector3(0, 0, transform.position.z);
        desiredZoom = transform.position.z;
    }

    public void OnCameraZoom(float value)
    {
        float currentZoom = transform.position.z;
        if ((value > 0 && desiredZoom < currentZoom) || (value < 0 && desiredZoom > currentZoom))
        {
            //if we swap directions we want that to immediately begin to work rather than trying to counteract
            desiredZoom = currentZoom;
        }
        desiredZoom = Mathf.Clamp(desiredZoom + value * zoomSensitivity, minMaxZValue.x, minMaxZValue.y);
    }

    void Update()
    {
        localOffset = Vector3.Lerp(localOffset, new Vector3(localOffset.x, localOffset.y, desiredZoom), Time.deltaTime * zoomLerpSpeed);
    }

    private void LateUpdate()
    {
        transform.position = player.transform.position + localOffset;
    }
}
