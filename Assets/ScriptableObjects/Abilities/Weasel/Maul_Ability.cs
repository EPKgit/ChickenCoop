using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Maul_Ability : Ability
{
    public float damageInitial = 10.0f;
    public float damageFinal = 20.0f;
    public float hitboxDuration = 0.25f;
    public HitboxChainAsset customChain;
    
    private HitboxChainHandle handle;
    private Vector2 startPosition;
    private float startRotation;

    public override void Initialize(PlayerAbilities pa)
	{
		base.Initialize(pa);
    }

    protected override void UseAbility()
	{
        base.UseAbility();
        startPosition = targetingData.inputPoint = ClampPointWithinRange(targetingData.inputPoint, 0.5f);
        startRotation = targetingData.inputRotationZ;
        handle = HitboxManager.instance.StartHitboxChain(customChain, HitboxPositionCallback, HitboxRotationCallback, HitboxCallback);
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

    protected override bool OverrideSetTickingAbility()
    {
        return true;
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
