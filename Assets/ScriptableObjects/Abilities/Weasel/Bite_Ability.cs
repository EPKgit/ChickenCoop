using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Bite_Ability : Ability
{
    public float damage = 1.0f;
    public float hitboxDuration = 0.25f;
    public float hitboxRadius = 0.5f;

    private StatBlock statBlock;
    public override void Initialize(PlayerAbilities pa)
	{
		base.Initialize(pa);
        statBlock = pa.stats;
    }

    public override void FinishAbility()
    {
        base.FinishAbility();
    }

    protected override void UseAbility()
	{
        base.UseAbility();
        targetingData.inputPoint = ClampPointWithinRange(targetingData.inputPoint, targetingData.range - 0.5f);
        HitboxManager.instance.SpawnHitbox
        (
            HitboxData.GetBuilder()
                .Duration(hitboxDuration)
                .StartPosition(targetingData.inputPoint)
                .Callback(HitboxCallback)
                .Discriminator(HitboxManager.Discriminators.Damagables)
                .RepeatPolicy(HitboxRepeatPolicy.ONLY_ONCE)
                .Radius(0.5f)
                .Finalize()
        );
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
