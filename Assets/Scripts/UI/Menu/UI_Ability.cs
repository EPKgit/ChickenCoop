using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Ability : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler 
{
    private UI_Slot controllingSlot;

    private RectTransform rect;
    private CanvasGroup canvas;
    private Image image;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponent<CanvasGroup>();
        image = GetComponent<Image>();
        image.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvas.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvas.blocksRaycasts = true;
    }

    public void SetSlot(GameObject g, bool canSwap)
    {
        SetSlot(g.GetComponent<UI_Slot>(), canSwap);
    }
    public void SetSlot(UI_Slot slot, bool canSwap)
    {
        if(slot == null)
        {
            return;
        }
        if(slot.controlledAbility != null && canSwap) //need to do a swap
        {
            slot.controlledAbility.SetSlot(controllingSlot, false);
        }
        slot.controlledAbility = this;
        controllingSlot = slot;
        transform.SetParent(slot.transform, false);
        rect.localPosition = Vector3.zero;
    }
}
