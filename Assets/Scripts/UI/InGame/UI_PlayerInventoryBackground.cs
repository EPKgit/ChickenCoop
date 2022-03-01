using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_PlayerInventoryBackground : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData data)
    {
        data.pointerDrag?.GetComponent<UI_Ability>()?.ConfirmDrop();
    }
}
