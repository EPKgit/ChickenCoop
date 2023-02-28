using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Maul_Ability : Ability
{
    public float damage = 20.0f;
    public float hitboxDuration = 0.25f;
    public HitboxChainAsset customChain;
    public HitboxChainHandle handle;

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

    void HitboxCallback(Collider2D col)
    {
        col.GetComponent<IDamagable>().Damage
        (
            HealthChangeData.GetBuilder()
                .Damage(damage)
                .BothSources(playerAbilities.gameObject)
                .KnockbackData(KnockbackPreset.MEDIUM)
                .Target(col.gameObject)
                .Finalize()
        );
    }
}
