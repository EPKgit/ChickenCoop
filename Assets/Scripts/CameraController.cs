using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject[] players;
    public Vector2 minMaxZValue = new Vector2(-20.0f, -3.0f);
    public float zoomSensitivity = 1.0f;
    public float zoomLerpSpeed = 3.0f;

    private Vector3 localOffset;
    private float desiredZoom;

    private void Awake()
    {
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
    }
}
