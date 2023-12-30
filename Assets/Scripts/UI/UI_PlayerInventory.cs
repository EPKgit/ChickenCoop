using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup), typeof(RectTransform))]
public class UI_PlayerInventory : MonoBehaviour
{
    public GameObject slotPrefab;
    public GameObject abilityPrefab;


    private List<GameObject> slots = new List<GameObject>();
    private List<GameObject> playerAbilityObjects = new List<GameObject>();

    private PlayerAbilities playerAbilities;

    private RectTransform rectTransform;
    private GridLayoutGroup gridGroup;
    private GameObject playerInventoryGameObject;

    private float cachedX, cachedY;

    private void Awake()
    {
        playerInventoryGameObject = transform?.parent?.parent?.gameObject;
        if (playerInventoryGameObject != null)
        {
            playerInventoryGameObject.gameObject.SetActive(false);
        }
        gridGroup = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
        
        cachedX = cachedY = -1;
    }
    public void Setup(PlayerAbilities pa)
    {
        playerAbilities = pa;
        Recalculate();
    }

    private int delayedRecalculatePendingFrames = -1;
    public void RecalculateDelayed(int frames = 0)
    {
        if(delayedRecalculatePendingFrames == -1)
        {
            delayedRecalculatePendingFrames = frames;
        }
        else
        {
            delayedRecalculatePendingFrames = Mathf.Max(delayedRecalculatePendingFrames, frames);
            StopCoroutine(RecalculateDelayedCoroutine());
        }
        StartCoroutine(RecalculateDelayedCoroutine());
    }

    private IEnumerator RecalculateDelayedCoroutine()
    {
        while(delayedRecalculatePendingFrames > -1)
        {
            --delayedRecalculatePendingFrames;
            yield return null;
        }
        Recalculate();
        delayedRecalculatePendingFrames = -1;
    }

    public void Recalculate()
    {
        for (int x = slots.Count - 1; x >= 0; --x)
        {
            Destroy(slots[x]);
        }
        slots.Clear();
        for (int x = playerAbilityObjects.Count - 1; x >= 0; --x)
        {
            Destroy(playerAbilityObjects[x]);
        }
        playerAbilityObjects.Clear();
        GenerateSlots();
        GenerateAbilities();
    }

    private void Update()
    {
        Rect rect = rectTransform.rect;
        if (rect.width != cachedX || rect.height != cachedY)
        {
            cachedX = rect.width;
            cachedY = rect.height;
            GridSizing();
        }
    }

    void GenerateSlots()
    {
        for (int x = 0; x < AbilitySlot.MAX.AsInt(); ++x)
        {
            UI_Slot temp = Instantiate(slotPrefab).GetComponent<UI_Slot>();
            temp.abilitySlotIndex = (AbilitySlot)x;
            temp.OnAbilityDropped += OnDropAbility;
            slots.Add(temp.gameObject);
            temp.transform.SetParent(transform, false);
            temp.name = "SLOT " + x;
            temp.controller = this;
        }
        GridSizing();
    }

    private void GridSizing()
    {
        Rect rect = rectTransform.rect;
        if (playerAbilities == null)
        {
            return;
        }
        var w = rect.width;
        var h = rect.height / (float)(playerAbilities.abilities.Length + 1);
        // if (w > h)
        {
            gridGroup.cellSize = new Vector2(w, h);
        }
        // else
        // {
        //     gridGroup.cellSize = new Vector2(w, w);
        // }
        gridGroup.spacing = new Vector2(w, h / (float)playerAbilities.abilities.Length);
        transform.hasChanged = false;
    }

    void GenerateAbilities()
    {
        for (int x = 0; x < playerAbilities.abilities.Length; ++x)
        {
            UI_Ability temp = Instantiate(abilityPrefab).GetComponent<UI_Ability>();
            temp.Setup(playerAbilities.abilities[x], null);
            playerAbilityObjects.Add(temp.gameObject);
            temp.SetSlot(slots[x], false);
            temp.name = "ABILITY " + x;
        }
    }

    void OnDropAbility(AbilitySlot newAbilitySlot, AbilitySlot oldAbilitySlot, UI_Ability previousAbility, UI_Ability newAbility)
    {
        if (newAbility?.ability == null)
        {
            Debug.LogError("Attempted to drop null ability in slot");
            return;
        }
        if (newAbilitySlot == AbilitySlot.INVALID)
        {
            Debug.LogError("Attempted to drop ability into invalid slot");
            return;
        }
        if(newAbilitySlot == AbilitySlot.DROPPED_ABILITY)
        {
            // newAbility.CreateDroppedAbilityObject(null, true);
            return;
        }
        InGameUIManager.instance.RevokeCallbacks();
        playerAbilities.abilities.SetSlot(newAbilitySlot, newAbility?.ability);
        playerAbilities.abilities.SetSlot(oldAbilitySlot, previousAbility?.ability);
        if(newAbility != null && newAbility.droppedAbilityObject != null)
        {
            Destroy(newAbility.droppedAbilityObject);
            newAbility.droppedAbilityObject = null;
        }
        Recalculate();
        InGameUIManager.instance.ReinitializeUI();
    }
}
