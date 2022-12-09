using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : Poolable
{
    public GameObject bulletPrefab;

    public float lifetime;
    public float damage;
    public float bulletSpeed;
    public float bulletLifetime;
    public float attackInterval;
    public float range;

    public LayerMask layerToScan;

    private PoolLoanToken token;
    private float lifeTimer;
    private float shootTimer;

    public void Start()
    {
        token = PoolManager.instance.RequestLoan(bulletPrefab, Mathf.CeilToInt(bulletLifetime / attackInterval) + 1, true);
    }

    public override void Reset()
    {
        base.Reset();
        lifeTimer = 0;
        shootTimer = attackInterval;
    }

    public void Update()
    { 
        lifeTimer += Time.deltaTime;
        shootTimer -= Time.deltaTime;
        if(shootTimer < 0)
        {
            if(Shoot())
            {
                shootTimer = attackInterval;
            }
        }
        if(lifeTimer > lifetime)
        {
            DestroySelf();
        }
    }

    bool Shoot()
    {
        var potentials = Physics2D.OverlapCircleAll(transform.position, range, layerToScan.value);
        float closestDistance = float.MaxValue;
        GameObject bestOption = null;
        foreach(var i in potentials)
        {
            var damagable = Lib.LibGetComponent<IDamagable>(i.gameObject);
            if (damagable != null)
            {
                float dist = Vector2.Distance(transform.position, i.transform.position);
                if (dist < closestDistance)
                {
                    bestOption = i.gameObject;
                    closestDistance = dist;
                }
            }
        }
        if(bestOption == null)
        {
            return false;
        }
        GameObject instance = PoolManager.instance.RequestObject(bulletPrefab);
        instance.GetComponent<Bullet>().Setup(transform.position, (bestOption.transform.position - transform.position).normalized * bulletSpeed, gameObject, damage, bulletLifetime);
        return true;
    }
}
