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

    //public MasterControls controls;
	public InputDevice inputDevice;
	//InputUser inputUser;
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
		//inputUser = new InputUser();
		//controls.MakePrivateCopyOfActions();
		//controls.Gameplay.SetCallbacks(this);

		GetPlayerID();
		gameObject.name = "Player " + playerID;

		if(testingController || testingMouseAndKeyboard)
		{
			SetupTestingDevices();
		}
		else
		{
			SetupInGameDevices();
		}
		//controls.Enable();
	}

	void SetupTestingDevices()
	{
		if(testingController)
		{
			inputDevice = InputSystem.GetDevice<Gamepad>();
			gamepad = inputDevice as Gamepad;
			inputType = InputType.GP;
		}
		else if(testingMouseAndKeyboard)
		{
			inputDevice = InputSystem.GetDevice<Keyboard>();
			keyboard = inputDevice as Keyboard;
			mouse = InputSystem.GetDevice<Mouse>();
			inputType = InputType.KB;
			//inputUser = InputUser.PerformPairingWithDevice(mouse, inputUser);
		}
		//inputUser =	InputUser.PerformPairingWithDevice(inputDevice, inputUser);
		//inputUser.AssociateActionsWithUser(controls);
	}

	void SetupInGameDevices()
	{
		if(inputDevice is Gamepad)
		{
			gamepad = inputDevice as Gamepad;
			inputType = InputType.GP;
		}
		else
		{
			keyboard = inputDevice as Keyboard;
			mouse = InputSystem.GetDevice<Mouse>();
			inputType = InputType.KB;
			//inputUser = InputUser.PerformPairingWithDevice(mouse, inputUser);
		}
		//inputUser =	InputUser.PerformPairingWithDevice(inputDevice, inputUser);
		//inputUser.AssociateActionsWithUser(controls);
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

	// bool IsMyInput(InputAction.CallbackContext ctx)
	// {
	// 	if(ctx.action.lastTriggerControl.device == inputDevice || (inputType == InputType.KB && mouse == ctx.action.lastTriggerControl.device ))
	// 	{
	// 		Debug.Log("true");
	// 		return true;
	// 	}
	// 	Debug.Log("false");
	// 	return false;
	// }

	#region MOVEMENT

	public void OnMovement(InputAction.CallbackContext ctx)
	{
		if(DEBUGFLAGS.MOVEMENT) Debug.Log(gameObject.name + " MOVING ");
		DoMovement(ctx);
	}

	void DoMovement(InputAction.CallbackContext ctx)
	{
		//playerMovement.Move(Lib.GetInputDirection(gamepad, mouse, ctx, inputType, gameObject, true));
	}

	#endregion


	#region AIMDIRECTION

	public void OnAimDirection(InputAction.CallbackContext ctx)
	{	
		if(DEBUGFLAGS.AIMING) Debug.Log(gameObject.name + " AIMING");
		DoAimDirection(ctx);
	}

	void DoAimDirection(InputAction.CallbackContext ctx)
	{
		//aimDirection = Lib.GetInputDirection(gamepad, mouse, ctx, inputType, gameObject);
	}

  public Vector2 GetAimDirection() {
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
		//playerAbilities.Attack(ctx, Lib.GetInputDirection(gamepad, mouse, ctx, inputType, gameObject));
	}

	#endregion

	#region ABILITIES

	public void OnAbility1(InputAction.CallbackContext ctx)
	{
		//playerAbilities.Ability1(ctx, Lib.GetInputDirection(gamepad, mouse, ctx, inputType, gameObject));
	}

	public void OnAbility2(InputAction.CallbackContext ctx)
	{
		//playerAbilities.Ability2(ctx, Lib.GetInputDirection(gamepad, mouse, ctx, inputType, gameObject));
	}

	public void OnAbility3(InputAction.CallbackContext ctx)
	{
		//playerAbilities.Ability3(ctx, Lib.GetInputDirection(gamepad, mouse, ctx, inputType, gameObject));
	}

	#endregion

	#region INTERACTION

	public void OnInteract(InputAction.CallbackContext ctx)
	{
		playerInteraction.AttemptPerform();
	}

	#endregion
}
