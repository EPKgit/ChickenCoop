using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Scurry_Ability : Ability
{
    public float movementSpeedMultiplier = 1.5f;

    private uint? statusHandle;
    private StatBlock statBlock;
    public override void Initialize(PlayerAbilities pa)
	{
		base.Initialize(pa);
        statBlock = pa.stats;
    }

    public override void FinishAbility()
    {
        base.FinishAbility();
        if(statusHandle.HasValue)
        {
            statBlock.GetStat(StatName.MovementSpeed)?.RemoveMultiplicativeModifier(statusHandle.Value);
            statusHandle = null;
        }
    }

    protected override void UseAbility()
	{
        base.UseAbility();
        statusHandle = statBlock.GetStat(StatName.MovementSpeed)?.AddMultiplicativeModifier(movementSpeedMultiplier);
    }
}
