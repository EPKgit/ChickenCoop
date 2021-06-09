using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashAttack : Ability, IMovementAbility
{
    public float damage;

    private Vector2 startPosition;
    private Vector2 destination;
    private Vector2 prevPosition;

    public float movementDuration
    {
        get
        {
            return maxDuration;
        }
        set
        {
            maxDuration = value;
        }
    }

    public float movementDistance
    {
        get
        {
            return targetingData.range;
        }
        set
        {
            targetingData.range = value;
        }
    }

    public override string GetTooltip()
    {
        return string.Format(tooltipDescription, movementDistance, movementDuration);
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
        playerAbilities.GetComponent<PlayerMovement>()?.DashInput(startPosition, destination, maxDuration);
    }
}

