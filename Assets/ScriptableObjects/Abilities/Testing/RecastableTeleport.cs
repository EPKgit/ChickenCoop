using UnityEngine;

public class RecastableTeleport : Ability
{   
	private Vector3 startingPosition;

    public MovementType type => MovementType.TELEPORT;

    public override void Reinitialize()
	{
		base.Reinitialize();
	}

    protected override void UseAbility()
	{
        base.UseAbility();
        startingPosition = playerAbilities.transform.position;
        playerAbilities.movement.TeleportInput(targetingData.inputPoint);
        SwitchTargetingType(1);
    }

	protected override void ReuseAbility(int recastNumber)
	{
        base.UseAbility();
        playerAbilities.movement.TeleportInput(startingPosition);
        SwitchTargetingType(0);
    }
}