using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject[] players;
    public Vector2 minMaxOrthographicSize = new Vector2(3, 8);
    public float zoomSensitivity = 1.0f;
    public float zoomLerpSpeed = 3.0f;

    private Camera attachedCamera;
    private Vector3 localOffset;
    private float desiredZoom;
    private float currentZoom;

    private void Awake()
    {
        attachedCamera = GetComponent<Camera>();
        if (players == null || players.Length == 0)
        {
            players = GameObject.FindGameObjectsWithTag("Player");
            if (players == null)
            {
                throw new System.Exception("ERROR: NO PLAYERS FOUND ON CAMERA");
            }
        }
        int index = 0;
        foreach (GameObject p in players)
        {
            PlayerInput input = p.GetComponent<PlayerInput>();
            if(!input)
            {
                players[index] = null;
            }
            else
            {
                input.SetCamera(this);
            }
            ++index;
        }
        localOffset = new Vector3(0, 0, transform.position.z);
        desiredZoom = currentZoom = attachedCamera.orthographicSize;
    }

    public void OnCameraZoom(float value)
    {
        if ((value > 0 && desiredZoom < currentZoom) || (value < 0 && desiredZoom > currentZoom))
        {
            //if we swap directions we want that to immediately begin to work rather than trying to counteract
            desiredZoom = currentZoom;
        }
        desiredZoom = Mathf.Clamp(desiredZoom + value * -zoomSensitivity, minMaxOrthographicSize.x, minMaxOrthographicSize.y);
    }

    void Update()
    {
        currentZoom = Mathf.Lerp(currentZoom, desiredZoom, Time.deltaTime * zoomLerpSpeed);
    }

    private void LateUpdate()
    {
        Vector3 avgPosition = Vector3.zero;
        int count = 0;
        foreach(GameObject p in players)
        {
            if (p != null)
            {
                avgPosition += p.transform.position;
                ++count;
            }
        }

        transform.position = (avgPosition / count) + localOffset;
        attachedCamera.orthographicSize = currentZoom;
    }
}
