using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Maul_Ability : Ability
{
    public float damage = 20.0f;
    public float hitboxDuration = 0.25f;
    public HitboxChainAsset customChain;

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
        HitboxManager.instance.StartHitboxChain(customChain, HitboxPositionCallback, HitboxRotationCallback, HitboxCallback);
    }

    Vector2 HitboxPositionCallback()
    {
        return startPosition;
    }

    float HitboxRotationCallback()
    {
        return startRotation;
    }

    void HitboxCallback(Collider2D col)
    {
        col.GetComponent<IDamagable>().Damage
        (
            HealthChangeData.GetBuilder()
                .Damage(damage)
                .BothSources(playerAbilities.gameObject)
                .Target(col.gameObject)
                .Finalize()
        );
    }
}
