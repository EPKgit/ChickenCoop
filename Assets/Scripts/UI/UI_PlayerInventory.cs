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
    private GameObject groundInventoryGameObject;

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
        GenerateSlots(groundAbilities);
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

    void GenerateSlots(List<GameObject> groundAbilities)
    {
        int x = 0;
        for (x = 0; x < playerAbilities.abilities.Length; ++x)
        {
            UI_Slot temp = Instantiate(slotPrefab).GetComponent<UI_Slot>();
            temp.index = x;
            temp.onAbilityDropped += OnDropAbility;
            slots.Add(temp.gameObject);
            temp.transform.SetParent(transform, false);
            temp.name = "SLOT " + x;
        }
        int offset = x;
        for (x = 0; x < groundAbilities.Count; ++x)
        {
            UI_Slot temp = Instantiate(slotPrefab).GetComponent<UI_Slot>();
            temp.index = x + offset;
            temp.onAbilityDropped += OnDropAbility;
            slots.Add(temp.gameObject);
            temp.transform.SetParent(groundInventoryGameObject.transform, false);
            temp.name = "GROUND SLOT " + x;
        }
        GridSizing();
    }

    private void GridSizing()
    {
        Debug.Log("sizing");
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

    void OnDropAbility(int index, Ability previousAbility, Ability newAbility)
    {
        InGameUIManager.instance.RevokeCallbacks();
        AbilitySetContainer set = playerAbilities.abilities;
        set[index] = newAbility;
        InGameUIManager.instance.ReinitializeUI();
    }

    void GenerateAbilities()
    {
        for (int x = 0; x < playerAbilities.abilities.Length; ++x)
        {
            UI_Ability temp = Instantiate(abilityPrefab).GetComponent<UI_Ability>();
            temp.Setup(playerAbilities.abilities[x]);
            playerAbilityObjects.Add(temp.gameObject);
            temp.SetSlot(slots[x], false);
            temp.name = "ABILITY " + x;
        }
    }
}
