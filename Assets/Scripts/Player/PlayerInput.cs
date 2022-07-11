﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
	public class InputEventData<T>
	{
        public bool handled = false;
        public T data;
		public InputEventData(T d)
		{
            data = d;
        }
    }
    public event Action<InputEventData<Vector2>> OnMoveEvent = delegate { };
    public event Action<InputEventData<Vector2>> OnAimPointEvent = delegate { };

    public event Action<InputEventData<bool>> OnInteractEvent = delegate { };
    public event Action<InputEventData<bool>> OnInventoryEvent = delegate { };



    public int playerID = -1;

	private PlayerMovement playerMovement;
	private PlayerAbilities playerAbilities;
	private PlayerInteraction playerInteraction;

    private Vector2 aimPointInput;
    public Vector2 aimPoint 
    { 
        get
        {
            if(aimDirty)
            {
                RecalcAimPoint();
            }
            return _aimPoint;
        }
        private set
        {
            _aimPoint = value;
            aimDirty = false;
        } 
    }
    private Vector2 _aimPoint;
    private bool aimDirty = false;

    #region MONOCALLBACKS

    void Awake()
	{
		playerMovement = GetComponent<PlayerMovement>();
		playerAbilities = GetComponent<PlayerAbilities>();
		playerInteraction = GetComponent<PlayerInteraction>();
	}

    void LateUpdate()
    {
        aimDirty = true;
    }

	#endregion

	#region MOVEMENT

	public void OnMovement(InputAction.CallbackContext ctx)
	{
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.MOVEMENT, gameObject.name + " MOVING ");
        Vector2 value = ctx.ReadValue<Vector2>();
        if (!TestOverride(OnMoveEvent, value))
        {
            playerMovement.MoveInput(value);
        }
    }

	#endregion


	#region AIMPOINT

	public void OnAimPoint(InputAction.CallbackContext ctx)
	{
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.AIMING, gameObject.name + " AIMING AT " + aimPoint);
        aimPointInput = ctx.ReadValue<Vector2>();
        RecalcAimPoint();
    }

    public void RecalcAimPoint()
    {
        aimPoint = Lib.GetAimPoint(aimPointInput);
        OnAimPointEvent.Invoke(new InputEventData<Vector2>(aimPoint));
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
        if (ctx.performed && !TestOverride(OnInteractEvent, ctx.performed))
        {
            playerInteraction.AttemptPerform();
        }
    }

	#endregion

    public void OnInventory(InputAction.CallbackContext ctx)
    {
        if(ctx.performed && !TestOverride(OnInventoryEvent, ctx.performed))
        {
            playerAbilities.ToggleInventory();
        }
    }

	private bool TestOverride<T>(Action<InputEventData<T>> action, T d)
	{
        InputEventData<T> data = new InputEventData<T>(d);
        action.Invoke(data);
        return data.handled;
    }
}
