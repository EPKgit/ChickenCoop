using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class SpineBackpack_Script : Poolable
{
    public GameObject spinePrefab;

    private float timeLeftMax = 30.0f;
    private float timeLeftCurrent;

    private new Collider2D collider2D;

    private GameObject creator;
    private float projectileLifetime;
    private float projectileSpeed;
    private float damage;
    private float spineCount;
    private bool triggerableBySpine;

    // Blue Upgrade
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float launchTimeCurrent;
    private float launchTimeMax;
    private float arcSteepness;

    

    public override void PoolInit(GameObject g)
    {
        spinePrefab = AssetLibraryManager.instance.GetPrefab("spine", "porcupine");
        collider2D = GetComponent<Collider2D>();
        base.PoolInit(g);
    }

    public override void Reset()
    {
        base.Reset();
        collider2D.enabled = false;
    }

    public void Setup(Vector3 startPos, GameObject creator, float damage, float spineCount, float myLifetime, float projLifetime, float projSpeed, bool triggerableBySpine)
    {
        transform.rotation = Quaternion.identity;
        this.creator = creator;
        this.damage = damage;
        this.spineCount = spineCount;
        projectileSpeed = projSpeed;
        projectileLifetime = projLifetime;
        timeLeftMax = timeLeftCurrent = myLifetime;
        transform.position = startPos;
        startPosition = startPos;
        this.triggerableBySpine = triggerableBySpine;
    }

    public void SetupGrounded()
    {
        float radius = Lib.LibGetComponentDownTree<CircleCollider2D>(gameObject).radius;
        var initialOverlaps = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var collision in initialOverlaps)
        {
            if(CheckCollision(collision.gameObject))
            {
                break;
            }
        }
        collider2D.enabled = true;
        launchTimeMax = -1;
    }

    public void SetupLaunch(Vector3 endPos, float arcTime, float arcSteepness)
    {
        endPosition = endPos;
        launchTimeMax = arcTime;
        launchTimeCurrent = 0;
        this.arcSteepness = arcSteepness;
    }

    void Detonate()
    {
        collider2D.enabled = false;
        for (int i = 0; i < spineCount; ++i)
        {
            float radians = i * (Mathf.PI * 2 / spineCount);
            float x = Mathf.Sin(radians);
            float y = Mathf.Cos(radians);
            GameObject temp = PoolManager.instance.RequestObject(spinePrefab);
            temp.GetComponent<SpineProjectile_Script>().Setup
            (
                transform.position,
                new Vector2(x, y) * projectileSpeed,
                creator,
                damage,
                projectileLifetime
            );
        }
        DestroySelf();
    }

    bool CheckCollision(GameObject g)
    {
        if (g.CompareTag("Enemy"))
        {
            Detonate();
            return true;
        }
        if(triggerableBySpine && g.GetComponent<SpineProjectile_Script>() != null)
        {
            Detonate();
            return true;
        }
        return false;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        CheckCollision(col.gameObject);
    }

    void Update()
    {
        if(launchTimeMax > 0)
        {
            launchTimeCurrent += Time.deltaTime;
            if (launchTimeCurrent >= launchTimeMax)
            {
                transform.position = endPosition;
                launchTimeMax = -1;
                SetupGrounded();
            }
            else
            {
                transform.position = Lib.GetArcPosition(startPosition, endPosition, launchTimeCurrent, launchTimeMax, arcSteepness);
            }
            return;
        }

        timeLeftCurrent -= Time.deltaTime;
        if (timeLeftCurrent <= 0)
        {
            Detonate();
        }
    }
}