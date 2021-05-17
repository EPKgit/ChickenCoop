using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashAttack : Ability
{
    public float damage;

    private Vector2 startPosition;
    private Vector2 destination;
    private Vector2 prevPosition;
    public override void Initialize(PlayerAbilities pa)
    {
        base.Initialize(pa);
    }

    public override void Cleanup(PlayerAbilities pa)
    {
        destination = Vector2.zero;
        base.Cleanup(pa);
    }

    protected override void UseAbility()
    {
        base.UseAbility();
        Vector2 direction = GetNormalizedDirectionTowardsTarget(targetingData.inputPoint);
        destination = (Vector2)playerAbilities.transform.position + (direction * targetingData.range);
        startPosition = playerAbilities.transform.position;
        
    }

    public override bool Tick(float deltaTime)
    {
        if(base.Tick(deltaTime))
        {
            return true;
        }
        prevPosition = playerAbilities.transform.position;
        playerAbilities.transform.position = Vector2.Lerp(startPosition, destination, 1.0f - (currentDuration / maxDuration));
        return false;
    }
}

