using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Slot : MonoBehaviour, IDropHandler
{
    public delegate void OnDropAbility(int index, Ability previousAbility, Ability newAbility);
    public event OnDropAbility onAbilityDropped = delegate { };

    public int index = 0;
    public UI_Ability ControlledAbility
    {
        get
        {
            return _controlledAbility;
        }
        set
        {
            onAbilityDropped(index, _controlledAbility?.ability, value?.ability);
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
            onAbilityDropped(index, _controlledAbility?.ability, ability.ability);
            ability.SetSlot(this, true);
        }
    }
}
