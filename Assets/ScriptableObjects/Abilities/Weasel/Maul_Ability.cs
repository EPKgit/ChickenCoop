using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class Maul_Ability : Ability
{
    public override uint ID => 104;

    public GameObject slashVFX;
    public GameObject biteVFX;

    public float damageInitial = 10.0f;
    public float damageFinal = 20.0f;
    public float hitboxDuration = 0.25f;
    public HitboxChainAsset customChain;
    
    private HitboxChainHandle handle;
    private Vector2 startPosition;
    private float startRotation;
    private PoolLoanToken token;

    public override void Initialize(PlayerAbilities pa)
    { 
        token = new PoolLoanToken(slashVFX, 4, true);
        base.Initialize(pa);
    }

    protected override void UseAbility()
	{
        base.UseAbility();
        startPosition = targetingData.inputPoint = ClampPointWithinRange(targetingData.inputPoint, 0.5f);
        startRotation = targetingData.inputRotationZ;
        handle = HitboxManager.instance.StartHitboxChain(customChain, HitboxPositionCallback, HitboxRotationCallback, HitboxCallback, HitboxSpawnCallback);
    }

    public override bool Tick(float deltaTime)
    {
        return handle.Completed;
    }

    Vector2 HitboxPositionCallback()
    {
        return startPosition;
    }

    float HitboxRotationCallback()
    {
        return startRotation;
    }

    void HitboxSpawnCallback(int index)
    {
        if (index < 2)
        {
            const float ARC_PERCENT = 0.3f;
            float first = index == 0 ? -1f : 1f;

            GameObject vfx = PoolManager.instance.RequestObject(slashVFX);
            vfx.transform.position = playerAbilities.transform.position;
            vfx.transform.rotation = Quaternion.Euler(0, 0, startRotation + first * (360 * ARC_PERCENT));
            vfx.transform.localScale = new Vector3(first, 1, 1);

            VisualEffect ve = vfx.GetComponent<VisualEffect>();
            ve.SetFloat("Lifetime", 0.2f);
            ve.SetFloat("ArcPercent", ARC_PERCENT);
        }
        else
        {
            GameObject vfx = PoolManager.instance.RequestObject(biteVFX);
            vfx.transform.position = startPosition;
            vfx.transform.rotation = Quaternion.Euler(0, 0, startRotation);
            vfx.transform.localScale = new Vector3(3, 5, 3);
        }
    }

    void HitboxCallback(Collider2D col, int index)
    {
        float dmg = index > 1 ? damageFinal : damageInitial;
        KnockbackPreset kb = index > 1 ? KnockbackPreset.BIG : KnockbackPreset.LITTLE;
        col.GetComponent<IDamagable>().Damage
        (
            HealthChangeData.GetBuilder()
                .Damage(dmg)
                .BothSources(playerAbilities.gameObject)
                .KnockbackData(kb)
                .Target(col.gameObject)
                .Finalize()
        );
    }
}
