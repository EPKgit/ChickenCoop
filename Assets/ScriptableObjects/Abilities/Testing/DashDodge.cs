using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashDodge : Ability
{
    public MovementType type => MovementType.DASH;

    private Vector2 startPosition;
    private Vector2 destination;
    private Vector2 prevPosition;

    public override string GetTooltip()
    {
        return string.Format(tooltipDescription, targetingData.range, maxDuration);
    }

    public override void Initialize(PlayerAbilities pa)
    {
        base.Initialize(pa);
    }

    public override void Cleanup(PlayerAbilities pa)
    {
        base.Cleanup(pa);
    }

    protected override void UseAbility()
    {
        base.UseAbility();
        Vector2 direction = GetNormalizedDirectionTowardsTarget(targetingData.inputPoint);
        destination = (Vector2)playerAbilities.transform.position + (direction * targetingData.range);
        startPosition = playerAbilities.transform.position;
        playerAbilities.movement.DashInput(startPosition, destination, maxDuration);
    }
}

