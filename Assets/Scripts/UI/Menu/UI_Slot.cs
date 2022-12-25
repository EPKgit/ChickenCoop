using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Slot : MonoBehaviour, IDropHandler
{
    public event Action<AbilitySlot, AbilitySlot, UI_Ability, UI_Ability> OnAbilityDropped = delegate { };

    /// <summary>
    /// Which slot does this ability represent in the UI for the player ability set
    /// </summary>
    public AbilitySlot abilitySlotIndex = AbilitySlot.INVALID;

    public UI_PlayerInventory controller;

    public UI_Ability ControlledAbility
    {
        get
        {
            return _controlledAbility;
        }
        set
        {
            OnAbilityDropped(abilitySlotIndex, AbilitySlot.INVALID, _controlledAbility, value);
            _controlledAbility = value;
        }
    }
    [SerializeField]
    private UI_Ability _controlledAbility;
    public void OnDrop(PointerEventData data)
    {
        UI_Ability ability = data.pointerDrag?.GetComponent<UI_Ability>();
        if (ability != null)    
        {
            OnAbilityDropped(abilitySlotIndex, ability.currentSlotIndex, _controlledAbility, ability);
            ability.SetSlot(this, true);
        }
    }

    public void Initialize(UI_Ability a)
    {
        _controlledAbility = a;
    }
}
