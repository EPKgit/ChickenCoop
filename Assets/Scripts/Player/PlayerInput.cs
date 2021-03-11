using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputType { KB, GP }

public class PlayerInput : MonoBehaviour
{
	public static List<PlayerInput> all = new List<PlayerInput>();

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
  	private Vector2 aimDirection;

	#region INIT

	void OnEnable()
	{
		all.Add(this);
		//controls.Enable();
	}
	void OnDisable()
	{
		all.Remove(this);
		//controls.Disable();
	}

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
			Initialize();
		}
	}

	public void Initialize()
    { 
		GetPlayerID();
		gameObject.name = "Player " + playerID;
	}


	void GetPlayerID()
	{
		if(playerID != -1)
		{
			return;
		}
		int max = 0;
		for(int x = 0; x < all.Count; ++x)
		{
			max = Mathf.Max(max, all[x].playerID);
		}
		playerID = max + 1;
	}

	#endregion

	#region MOVEMENT

	public void OnMovement(InputAction.CallbackContext ctx)
	{
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.MOVEMENT, gameObject.name + " MOVING ");
        playerMovement.Move(ctx.ReadValue<Vector2>());
	}

	#endregion


	#region AIMDIRECTION

	public void OnAimDirection(InputAction.CallbackContext ctx)
	{	
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.AIMING, gameObject.name + " AIMING");
        DoAimDirection(ctx);
	}

    void DoAimDirection(InputAction.CallbackContext ctx)
    {
        aimDirection = Lib.GetAimDirection(ctx, gameObject);
    }

    public Vector2 GetAimDirection()
    {
        return aimDirection;
    }

    #endregion

    #region ATTACK

    public void OnAttack(InputAction.CallbackContext ctx)
	{
		DoAttack(ctx);
	}

	void DoAttack(InputAction.CallbackContext ctx)
	{
		playerAbilities.Attack(ctx, aimDirection);
	}

	#endregion

	#region ABILITIES

	public void OnAbility1(InputAction.CallbackContext ctx)
	{
		playerAbilities.Ability1(ctx, aimDirection);
	}

	public void OnAbility2(InputAction.CallbackContext ctx)
	{
		playerAbilities.Ability2(ctx, aimDirection);
	}

	public void OnAbility3(InputAction.CallbackContext ctx)
	{
		playerAbilities.Ability3(ctx, aimDirection);
	}

	#endregion

	#region INTERACTION

	public void OnInteract(InputAction.CallbackContext ctx)
	{
		playerInteraction.AttemptPerform();
	}

	#endregion
}
