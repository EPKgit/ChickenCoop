using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Swipe_Ability : Ability
{
    public float damage = 20.0f;
    public float hitboxDuration = 0.25f;
    public HitboxDataAsset customHitbox;

    public override void Initialize(PlayerAbilities pa)
	{
		base.Initialize(pa);
        customHitbox = ScriptableObject.Instantiate(customHitbox);
        customHitbox.duration = hitboxDuration;
    }

    protected override void UseAbility()
	{
        base.UseAbility();
        targetingData.inputPoint = ClampPointWithinRange(targetingData.inputPoint, 0.5f);
        HitboxManager.instance.SpawnHitbox(customHitbox, targetingData.inputPoint, HitboxCallback, targetingData.inputRotationZ);
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
