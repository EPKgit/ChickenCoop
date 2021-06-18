using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousBulletSpawner : MonoBehaviour
{
    public GameObject bulletPrefab;

    public Vector2 direction;
    public float speed;
    public float damage;
    public float interval;
    public float lifetime;

    private float timer;

    private PoolLoanToken token;

    public void Start()
    {
        token = PoolManager.instance.RequestLoan(bulletPrefab, Mathf.CeilToInt(lifetime / interval) + 1, true);
    }

    public void Update()
    { 
        timer += Time.deltaTime;
        if(timer > interval)
        {
            timer -= interval;
            GameObject instance = PoolManager.instance.RequestObject(bulletPrefab);
            instance.GetComponent<Bullet>().Setup(transform.position, direction * speed, gameObject, damage, lifetime);
        }
    }
}
