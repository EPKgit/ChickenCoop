using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Scurry_Ability : Ability
{
    public float movementSpeedMultiplier = 1.5f;
    public float damageMultiplier = 2.0f;

    private uint? movementSpeedBonusHandle;
    private uint? dmgStatusHandle;
    private StatBlock statBlock;
    public override void Initialize(PlayerAbilities pa)
	{
		base.Initialize(pa);
        statBlock = pa.stats;
        pa.postAbilityCastEvent += OnPostAbilityCast;
    }
    
    public override void Cleanup(PlayerAbilities pa)
    {
        base.Cleanup(pa);
        pa.postAbilityCastEvent -= OnPostAbilityCast;
    }

    protected override void UseAbility()
    {
        base.UseAbility();
        movementSpeedBonusHandle = statBlock.GetStat(StatName.MovementSpeed)?.AddMultiplicativeModifier(movementSpeedMultiplier);
        dmgStatusHandle = statBlock.GetStat(StatName.DamagePercentage)?.AddMultiplicativeModifier(damageMultiplier);
    }

    void OnPostAbilityCast(AbilityEventData aed)
    {
        if(aed.ability.abilityTags.Contains(GameplayTagFlags.ABILITY_DAMAGE))
        {
            playerAbilities.AbilityEndedExternal(this);
        }
    }

    public override void FinishAbility()
    {
        base.FinishAbility();
        if(movementSpeedBonusHandle.HasValue)
        {
            statBlock.GetStat(StatName.MovementSpeed)?.RemoveMultiplicativeModifier(movementSpeedBonusHandle.Value);
            movementSpeedBonusHandle = null;
        }
        if (dmgStatusHandle.HasValue)
        {
            statBlock.GetStat(StatName.DamagePercentage)?.RemoveMultiplicativeModifier(dmgStatusHandle.Value);
            dmgStatusHandle = null;
        }
    }
}
