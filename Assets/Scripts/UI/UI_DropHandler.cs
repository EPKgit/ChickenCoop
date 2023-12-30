using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_DropHandler : MonoBehaviour, IDropHandler
{
    /// <summary>
    /// Guaranteed not to be null
    /// </summary>
    public event Action<GameObject> OnItemDropped = delegate { };

    public void OnDrop(PointerEventData data)
    {
        GameObject droppedItem = data.pointerDrag;
        if (droppedItem != null)    
        {
            OnItemDropped(droppedItem);
        }
    }
}
