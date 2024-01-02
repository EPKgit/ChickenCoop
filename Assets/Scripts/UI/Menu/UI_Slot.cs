using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class UI_Slot : MonoBehaviour
{
    public event Action<AbilitySlot, AbilitySlot, UI_Ability, UI_Ability> OnAbilityDropped = delegate { };

    public UI_DropHandler dropTarget;
    public TextMeshProUGUI titleTextComponent;
    public TextMeshProUGUI tooltipTextComponent;
    public Toggle[] upgradeButtons;

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

        if (upgradeButtons.Length != Ability.AbilityUpgradeSlot.MAX.intValue())
        {
            Debug.LogError("Error: not enough buttons supplied to match the number of upgrades");
        }
        for(int x = 0; x < upgradeButtons.Length; ++x)
        {
            int lambdaCapture = x;
            upgradeButtons[x].onValueChanged.AddListener((state) => OnUpgradeButtonPressed(lambdaCapture, state));
        }
    }

    public void OnUpgradeButtonPressed(int index, bool state)
    {
        _controlledAbility.ability.ToggleAbilityUpgrade((Ability.AbilityUpgradeSlot)index);
        //if(_controlledAbility.ability.GetAbilityUpgradeStatus(Ability.AbilityUpgradeSlot)index)))
        //{
            //upgradeButtons[index].
        //}
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
            for(int x = 0; x < Ability.AbilityUpgradeSlot.MAX.intValue(); ++x)
            {
                upgradeButtons[x].SetIsOnWithoutNotify(_controlledAbility.ability.GetAbilityUpgradeStatus((Ability.AbilityUpgradeSlot)x));
            }
        }
    }
}
