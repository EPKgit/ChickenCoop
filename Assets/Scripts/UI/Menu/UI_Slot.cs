using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Slot : MonoBehaviour, IDropHandler
{
    public UI_Ability controlledAbility;
    public void OnDrop(PointerEventData data)
    {
        UI_Ability ability = data.pointerDrag?.GetComponent<UI_Ability>();
        if (ability != null)
        {
            ability.SetSlot(this, true);
        }
    }
}
