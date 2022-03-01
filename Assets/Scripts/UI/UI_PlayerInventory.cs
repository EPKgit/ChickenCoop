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
    private List<GameObject> groundAbilityObjects = new List<GameObject>();

    private PlayerAbilities playerAbilities;

    private RectTransform rectTransform;
    private GridLayoutGroup gridGroup;
    private GameObject playerInventoryGameObject;
    private GameObject groundInventoryGameObject;

    private float cachedX, cachedY;

    private void Awake()
    {
        playerInventoryGameObject = transform?.parent?.parent?.gameObject;
        if (playerInventoryGameObject != null)
        {
            playerInventoryGameObject.gameObject.SetActive(false);
        }
        groundInventoryGameObject = transform.parent.GetChild(1).gameObject;
        gridGroup = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
        
        cachedX = cachedY = -1;
    }
    public void Setup(PlayerAbilities pa, List<GameObject> groundAbilities)
    {
        playerAbilities = pa;
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
        groundAbilityObjects.Clear();
        GenerateSlots(groundAbilities);
        GenerateAbilities(groundAbilities);
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

    void GenerateSlots(List<GameObject> groundAbilities)
    {
        int x = 0;
        for (x = 0; x < AbilitySlots.MAX.AsInt(); ++x)
        {
            UI_Slot temp = Instantiate(slotPrefab).GetComponent<UI_Slot>();
            temp.abilitySlotIndex = (AbilitySlots)x;
            temp.onAbilityDropped += OnDropAbility;
            slots.Add(temp.gameObject);
            temp.transform.SetParent(transform, false);
            temp.name = "SLOT " + x;
        }
        int offset = x;
        for (x = 0; x < groundAbilities.Count; ++x)
        {
            UI_Slot temp = Instantiate(slotPrefab).GetComponent<UI_Slot>();
            temp.abilitySlotIndex = AbilitySlots.DROPPED_ABILITY;
            temp.onAbilityDropped += OnDropAbility;
            slots.Add(temp.gameObject);
            temp.transform.SetParent(groundInventoryGameObject.transform, false);
            temp.name = "GROUND SLOT " + x;
        }
        GridSizing();
    }

    private void GridSizing()
    {
        Rect rect = rectTransform.rect;
        if (playerAbilities == null)
            return;
        var w = rect.width / (float)(playerAbilities.abilities.Length + 1);
        var h = rect.height;
        if (w > h)
        {
            gridGroup.cellSize = new Vector2(h, h);
        }
        else
        {
            gridGroup.cellSize = new Vector2(w, w);
        }
        gridGroup.spacing = new Vector2(w / (float)playerAbilities.abilities.Length, h);
        transform.hasChanged = false;
    }

    void GenerateAbilities(List<GameObject> groundAbilities)
    {
        for (int x = 0; x < playerAbilities.abilities.Length; ++x)
        {
            UI_Ability temp = Instantiate(abilityPrefab).GetComponent<UI_Ability>();
            temp.Setup(playerAbilities.abilities[x], null);
            playerAbilityObjects.Add(temp.gameObject);
            temp.SetSlot(slots[x], false);
            temp.name = "ABILITY " + x;
        }
        int OFFSET = AbilitySlots.MAX.AsInt();
        for (int x = 0; x < groundAbilities.Count; ++x)
        {
            UI_Ability temp = Instantiate(abilityPrefab).GetComponent<UI_Ability>();
            DroppedAbility da = groundAbilities[x].GetComponent<DroppedAbility>();
            temp.Setup(da.ability, groundAbilities[x]);
            groundAbilityObjects.Add(temp.gameObject);
            temp.SetSlot(slots[x + OFFSET], false);
            temp.name = "ABILITY " + (x + OFFSET);
        }
    }

    void OnDropAbility(AbilitySlots abilitySlot, UI_Ability previousAbility, UI_Ability newAbility)
    {
        if (newAbility?.ability == null)
        {
            Debug.LogError("Attempted to drop null ability in slot");
            return;
        }
        if (abilitySlot == AbilitySlots.INVALID)
        {
            Debug.LogError("Attempted to drop ability into invalid slot");
            return;
        }
        if(abilitySlot == AbilitySlots.DROPPED_ABILITY)
        {
            newAbility.CreateDroppedAbilityObject(null, true);
            return;
        }
        InGameUIManager.instance.RevokeCallbacks();
        playerAbilities.abilities.SetSlot(abilitySlot, newAbility?.ability);
        if(newAbility != null && newAbility.droppedAbilityObject != null)
        {
            Destroy(newAbility.droppedAbilityObject);
            newAbility.droppedAbilityObject = null;
        }
        InGameUIManager.instance.ReinitializeUI();
    }
}
