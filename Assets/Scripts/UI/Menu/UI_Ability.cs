using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UI_Ability : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler

{
    public GameObject tooltipObject;
    public TextMeshProUGUI tooltipText;

    [HideInInspector]
    public Ability ability;

    private const float DROP_TIMER_RESET_COOLDOWN = 0.05f;

    [SerializeField]
    private UI_Slot controllingSlot;

    private RectTransform rect;
    private CanvasGroup canvas;
    private Image image;
    private float dropTimer;
    private Vector2 startPosition;
    private bool droppedOutsideUI;

    public void Setup(Ability a)
    {
        ability = a;
        image.sprite = ability.icon;
        image.color = Color.white;
    }

    #region MONOBEHAVIOUR_CALLBACKS
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponent<CanvasGroup>();
        image = GetComponent<Image>();
        if(ability == null)
        {
            image.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);
        }
        image.sprite = ability?.icon;
        tooltipObject.SetActive(false);
    }

    void OnEnable()
    {
        ((RectTransform)tooltipObject.transform).localPosition = new Vector2(0, ((RectTransform)transform).rect.height * 2);
    }

    void Update()
    {
        if (dropTimer == 0.0f)
        {
            return;
        }
        dropTimer -= Time.deltaTime;
        if (dropTimer < 0.0f)
        {
            dropTimer = 0.0f;
            rect.anchoredPosition = startPosition;
        }
    }
    #endregion

    #region POINTER_CALLBACKS
    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipText.text = ability.GetTooltip();
        tooltipObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipObject.SetActive(false);
    }
    #endregion

    #region DRAG_CALLBACKS
    public void OnBeginDrag(PointerEventData eventData)
    {
        tooltipObject.SetActive(false);
        if (dropTimer != 0.0f)
        {
            return;
        }
        canvas.blocksRaycasts = false;
        startPosition = rect.anchoredPosition;
        droppedOutsideUI = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvas.blocksRaycasts = true;
        dropTimer = DROP_TIMER_RESET_COOLDOWN;
        if(droppedOutsideUI)
        {
            Debug.Log("Empty Drop");
        }
    }

    #endregion

    public void ConfirmDrop()
    {
        droppedOutsideUI = false;
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
        if(slot.ControlledAbility != null && canSwap) //need to do a swap
        {
            slot.ControlledAbility.SetSlot(controllingSlot, false);
        }
        dropTimer = 0.0f;
        slot.ControlledAbility = this;
        controllingSlot = slot;
        transform.SetParent(slot.transform, false);
        rect.localPosition = Vector3.zero;
        ConfirmDrop();
    }
}
