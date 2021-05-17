using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PlayerInventory : MonoBehaviour
{
    public GameObject slotPrefab;
    public GameObject abilityPrefab;
    public GameObject inventoryGameObject;

    private List<GameObject> slots = new List<GameObject>();
    private List<GameObject> abilities = new List<GameObject>();

    private PlayerAbilities playerAbilities;
    public void Setup(PlayerAbilities pa)
    {
        playerAbilities = pa;
        for(int x = slots.Count - 1; x >= 0; --x)
        {
            Destroy(slots[x]);
        }
        slots.Clear();
        for (int x = abilities.Count - 1; x >= 0; --x)
        {
            Destroy(abilities[x]);
        }
        abilities.Clear();
        GenerateSlots();
        GenerateAbilities();
    }

    void GenerateSlots()
    {
        for (int x = 0; x < playerAbilities.abilities.Length; ++x)
        {
            UI_Slot temp = Instantiate(slotPrefab).GetComponent<UI_Slot>();
            temp.index = x;
            temp.onAbilityDropped += OnDropAbility;
            slots.Add(temp.gameObject);
            temp.transform.SetParent(inventoryGameObject.transform, false);
            temp.name = "SLOT " + x;
        }
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
            abilities.Add(temp.gameObject);
            temp.SetSlot(slots[x], false);
            temp.name = "ABILITY " + x;
        }
    }
}
