using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UI_Slot : MonoBehaviour
{
    public event Action<AbilitySlot, AbilitySlot, UI_Ability, UI_Ability> OnAbilityDropped = delegate { };

    public UI_DropHandler dropTarget;
    public TextMeshProUGUI titleTextComponent;
    public TextMeshProUGUI tooltipTextComponent;

    /// <summary>
    /// Which slot does this ability represent in the UI for the player ability set
    /// </summary>
    [HideInInspector] public AbilitySlot abilitySlotIndex = AbilitySlot.INVALID;

    [HideInInspector] public UI_PlayerInventory controller;

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
    private UI_Ability _controlledAbility;

    void Awake()
    {
        if(dropTarget == null)
        {
            Debug.LogError("ERROR: drop target for ability slot is not set");
        }
        dropTarget.OnItemDropped += OnDrop;
    }

    public void OnDrop(GameObject droppedObject)
    {
        UI_Ability ability = droppedObject.GetComponent<UI_Ability>();
        if (ability != null)    
        {
            OnAbilityDropped(abilitySlotIndex, ability.currentSlotIndex, _controlledAbility, ability);
            ability.SetSlot(this, true);
        }
    }

    public void Initialize(UI_Ability a)
    {
        _controlledAbility = a;
        if(_controlledAbility == null)
        {
            titleTextComponent.text = "";
            tooltipTextComponent.text = "";
        }
        else
        {
            titleTextComponent.text = _controlledAbility.ability.abilityName;
            tooltipTextComponent.text = _controlledAbility.ability.GetTooltip();
        }
    }
}
