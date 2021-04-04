using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputType { KB, GP }

public class PlayerInput : MonoBehaviour
{
	public bool testingController = false;
	public bool testingMouseAndKeyboard = false;

	public int playerID = -1;

	private InputType inputType;
	private Gamepad gamepad;
	private Keyboard keyboard;
	private Mouse mouse;

	private PlayerMovement playerMovement;
	private PlayerAbilities playerAbilities;
	private PlayerInteraction playerInteraction;
  	private Vector2 aimPoint;

	#region INIT

	void Awake()
	{
		playerMovement = GetComponent<PlayerMovement>();
		playerAbilities = GetComponent<PlayerAbilities>();
		playerInteraction = GetComponent<PlayerInteraction>();
		if(testingController || testingMouseAndKeyboard)
		{
			if(testingController && testingMouseAndKeyboard)
			{
				throw new System.InvalidOperationException("Cannot test both mouse+KB and controller on " + gameObject.name);
			}
		}
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
        DoAimPoint(ctx);
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.AIMING, gameObject.name + " AIMING AT " + aimPoint);

    }

    void DoAimPoint(InputAction.CallbackContext ctx)
    {
        aimPoint = Lib.GetAimPoint(ctx, gameObject);
    }

    public Vector2 GetAimPoint()
    {
        return aimPoint;
    }

    #endregion

    #region ATTACK

    public void OnAttack(InputAction.CallbackContext ctx)
	{
		DoAttack(ctx);
	}

	void DoAttack(InputAction.CallbackContext ctx)
	{
		playerAbilities.Attack(ctx, aimPoint);
	}

	#endregion

	#region ABILITIES

	public void OnAbility1(InputAction.CallbackContext ctx)
	{
		playerAbilities.Ability1(ctx, aimPoint);
	}

	public void OnAbility2(InputAction.CallbackContext ctx)
	{
		playerAbilities.Ability2(ctx, aimPoint);
	}

	public void OnAbility3(InputAction.CallbackContext ctx)
	{
		playerAbilities.Ability3(ctx, aimPoint);
	}

	#endregion

	#region INTERACTION

	public void OnInteract(InputAction.CallbackContext ctx)
	{
		playerInteraction.AttemptPerform();
	}

	#endregion
}
