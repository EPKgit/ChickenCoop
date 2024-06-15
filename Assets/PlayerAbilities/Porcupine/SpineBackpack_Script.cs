using System.Collections;
using System.Collections.Generic;
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
    private bool triggerableByAlly;

    // Red Upgrade
    private float setupTime = 0;
    private float shieldAmount;
    private KnockbackPreset knockbackModifier;

    // Blue Upgrade
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float launchTimeCurrent;
    private float launchTimeMax;
    private float arcSteepness;

    private HashSet<GameObject> pendingCollisions = new HashSet<GameObject>(); 
    

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

    public void Setup(Vector3 startPos, GameObject creator, float damage, float spineCount, float myLifetime, float projLifetime, float projSpeed)
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
        this.knockbackModifier = KnockbackPreset.MAX;
    }

    public void SetupRed(float setupTime, float shieldPercentage, KnockbackPreset knockbackModifier)
    {
        this.setupTime = setupTime;
        this.shieldAmount = shieldPercentage;
        this.knockbackModifier = knockbackModifier;
        this.triggerableByAlly = true;
    }

    public void SetupYellow()
    {
        this.triggerableBySpine = true;
    }

    public void SetupGrounded()
    {
        collider2D.enabled = true;
        launchTimeMax = -1;
        
        float radius = Lib.LibGetComponentDownTree<CircleCollider2D>(gameObject).radius;
        var initialOverlaps = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var collision in initialOverlaps)
        {
            if (setupTime > 0)
            {
                pendingCollisions.Add(collision.gameObject);
            }
            else
            {
                if (CheckCollision(collision.gameObject))
                {
                    break;
                }
            }
        }
        
    }

    public void SetupLaunch(Vector3 endPos, float arcTime, float arcSteepness)
    {
        endPosition = endPos;
        launchTimeMax = arcTime;
        launchTimeCurrent = 0;
        this.arcSteepness = arcSteepness;
    }

    void Detonate(GameObject ally)
    {
        collider2D.enabled = false;
        for (int i = 0; i < spineCount; ++i)
        {
            float radians = i * (Mathf.PI * 2 / spineCount);
            float x = Mathf.Sin(radians);
            float y = Mathf.Cos(radians);
            GameObject temp = PoolManager.instance.RequestObject(spinePrefab);
            var spine = temp.GetComponent<SpineProjectile_Script>();
            spine.Setup
            (
                transform.position,
                new Vector2(x, y) * projectileSpeed,
                creator,
                damage,
                projectileLifetime,
                knockbackModifier
            );
        }
        if(ally != null)
        {
            IShieldable shieldable = Lib.FindUpwardsInTree<IShieldable>(ally);
            if (shieldable != null)
            {
                shieldable.ApplyShield
                (
                    ShieldApplicationData.GetBuilder()
                        .LocalSource(gameObject)
                        .OverallSource(creator)
                        .Target(shieldable)
                        .Value(shieldAmount)
                        .Finalize()
                );
            }

            //Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, stunRadius);
            //foreach (Collider2D col in nearby)
            //{
            //    if (col.gameObject.CompareTag("Enemy"))
            //    {
            //        StatusEffectManager.instance.ApplyEffect(col.gameObject, Statuses.StatusEffectType.STUN, stunDuration);
            //    }
            //}
        }
        DestroySelf();
    }

    bool CheckCollision(GameObject g)
    {
        if (g.CompareTag("Enemy"))
        {
            Detonate(null);
            return true;
        }
        if(triggerableBySpine && g.GetComponent<SpineProjectile_Script>() != null)
        {
            Detonate(null);
            return true;
        }
        if (triggerableByAlly && g != creator && g.CompareTag("Player"))
        {
            Detonate(g);
            return true;
        }
        return false;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (setupTime > 0)
        {
            pendingCollisions.Add(col.gameObject);
            return;
        }
        CheckCollision(col.gameObject);
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (setupTime > 0)
        {
            pendingCollisions.Remove(col.gameObject);
            return;
        }
    }

    void Update()
    {
        if(setupTime > 0)
        {
            setupTime -= Time.deltaTime;
            if(setupTime <= 0)
            {
                setupTime = 0;
                foreach(GameObject g in pendingCollisions)
                {
                    if(CheckCollision(g.gameObject))
                    {
                        break;
                    }
                }
                pendingCollisions.Clear();
            }
        }
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
            Detonate(null);
        }
    }
}