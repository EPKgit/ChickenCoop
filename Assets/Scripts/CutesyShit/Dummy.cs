using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : MonoBehaviour
{
    private BaseHealth health;
    private Transform dummyTransform;
    private void Awake()
    {
        health = GetComponent<BaseHealth>();
        dummyTransform = transform.GetChild(1);
    }
    void OnEnable()
    {
        if(health)
        {
            health.healthChangeNotification += Shake;
        }
    }

    void OnDisable()
    {
        if (health)
        {
            health.healthChangeNotification -= Shake;
        }
    }

    public float shakeAngle = 60f;
    public float intensityAdded = 3f;
    public float intensityAddedWhenAlreadyMoving = 1f;
    public float shakeDecayModifier = 1.0f;

    private float shakeStrength = 0;
    private float shakeTime = -1;
    private bool returningToRest = false;
    void Shake(HealthChangeData hcd)
    {
        shakeStrength += shakeStrength != 0 ? intensityAddedWhenAlreadyMoving : intensityAdded;
    }

    private void Update()
    {
        if(returningToRest)
        {
            shakeTime += Time.deltaTime;
            dummyTransform.rotation = Quaternion.Euler(dummyTransform.rotation.x, dummyTransform.rotation.y, Mathf.Sin(shakeTime) * shakeAngle);
            if(shakeTime > Mathf.PI * 2)
            {
                returningToRest = false;
                shakeTime = -1;
            }
        }
        if (shakeStrength == 0)
        {
            return;
        }
        if(shakeTime == -1)
        {
            shakeTime = 0;
        }
        shakeTime += Time.deltaTime * shakeStrength;
        dummyTransform.rotation = Quaternion.Euler(dummyTransform.rotation.x, dummyTransform.rotation.y, Mathf.Sin(shakeTime) * shakeAngle);
        shakeStrength -= Time.deltaTime * shakeDecayModifier;
        if(shakeStrength < 0)
        {
            shakeStrength = 0;
            shakeTime %= (Mathf.PI * 2);
            returningToRest = true;
        }
    }
}
