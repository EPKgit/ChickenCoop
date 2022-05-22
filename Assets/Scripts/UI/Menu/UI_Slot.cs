using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Slot : MonoBehaviour, IDropHandler
{
    public delegate void OnDropAbility(AbilitySlots newAbilitySlot, AbilitySlots oldAbilitySlot, UI_Ability previousAbility, UI_Ability newAbility);
    public event OnDropAbility onAbilityDropped = delegate { };

    /// <summary>
    /// Which slot does this ability represent in the UI for the player ability set
    /// </summary>
    public AbilitySlots abilitySlotIndex = AbilitySlots.INVALID;

    public UI_PlayerInventory controller;

    public UI_Ability ControlledAbility
    {
        get
        {
            return _controlledAbility;
        }
        set
        {
            onAbilityDropped(abilitySlotIndex, AbilitySlots.INVALID, _controlledAbility, value);
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
            onAbilityDropped(abilitySlotIndex, ability.currentSlotIndex, _controlledAbility, ability);
            ability.SetSlot(this, true);
        }
    }

    public void Initialize(UI_Ability a)
    {
        _controlledAbility = a;
    }
}
