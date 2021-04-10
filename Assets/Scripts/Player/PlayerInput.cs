using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
	public int playerID = -1;

	private PlayerMovement playerMovement;
	private PlayerAbilities playerAbilities;
	private PlayerInteraction playerInteraction;

  	public Vector2 aimPoint { get; private set; }

	#region INIT

	void Awake()
	{
		playerMovement = GetComponent<PlayerMovement>();
		playerAbilities = GetComponent<PlayerAbilities>();
		playerInteraction = GetComponent<PlayerInteraction>();
	}

	#endregion

	#region MOVEMENT

	public void OnMovement(InputAction.CallbackContext ctx)
	{
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.MOVEMENT, gameObject.name + " MOVING ");
        playerMovement.Move(ctx.ReadValue<Vector2>());
	}

	#endregion


	#region AIMPOINT

	public void OnAimPoint(InputAction.CallbackContext ctx)
	{
        aimPoint = Lib.GetAimPoint(ctx, gameObject);
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.AIMING, gameObject.name + " AIMING AT " + aimPoint);
    }

    #endregion

    public void OnAttack(InputAction.CallbackContext ctx)
	{
        playerAbilities.AbilityInput(AbilitySlots.SLOT_ATTACK, ctx, aimPoint);
    }

	public void OnAbility1(InputAction.CallbackContext ctx)
	{
        playerAbilities.AbilityInput(AbilitySlots.SLOT_1, ctx, aimPoint);
    }

    public void OnAbility2(InputAction.CallbackContext ctx)
	{
        playerAbilities.AbilityInput(AbilitySlots.SLOT_2, ctx, aimPoint);
	}

	public void OnAbility3(InputAction.CallbackContext ctx)
	{
		playerAbilities.AbilityInput(AbilitySlots.SLOT_3, ctx, aimPoint);
	}

	#region INTERACTION

	public void OnInteract(InputAction.CallbackContext ctx)
	{
		playerInteraction.AttemptPerform();
	}

	#endregion
}
