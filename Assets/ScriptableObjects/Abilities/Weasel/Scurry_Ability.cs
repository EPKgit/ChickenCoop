using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Scurry_Ability : Ability
{
    public float movementSpeedMultiplier = 1.5f;
    public GameObject smokePrefab;

    private uint? statusHandle;
    private StatBlock statBlock;
    private PoolLoanToken token;
    public override void Initialize(PlayerAbilities pa)
	{
		base.Initialize(pa);
        statBlock = pa.stats;
        token = PoolManager.instance.RequestLoan(smokePrefab, 1, true);
    }

    public override void FinishAbility()
    {
        base.FinishAbility();
        EnemyManager.instance.ForceReevaluateEnemyAggro();
        if(statusHandle.HasValue)
        {
            statBlock.GetStat(StatName.MovementSpeed)?.RemoveMultiplicativeModifier(statusHandle.Value);
            statusHandle = null;
        }
    }

    protected override void UseAbility()
	{
        base.UseAbility();
        EnemyManager.instance.ForceReevaluateEnemyAggro();
        statusHandle = statBlock.GetStat(StatName.MovementSpeed)?.AddMultiplicativeModifier(movementSpeedMultiplier);
        PoolManager.instance.RequestObject(smokePrefab).transform.position = playerAbilities.transform.position;
    }
}
