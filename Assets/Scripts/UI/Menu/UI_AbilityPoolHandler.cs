using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_AbilityPoolHandler : MonoBehaviour
{
    public GameObject slotPrefab;
    public GameObject abilityPrefab;
    public GameObject poolGameObject;

    public int abilitiesToGenerate = 16;

    private List<GameObject> slots = new List<GameObject>();
    private List<GameObject> abilities = new List<GameObject>();

    void Start()
    {
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
        for(int x = 0; x < abilitiesToGenerate; ++x)
        {
            GameObject temp = Instantiate(slotPrefab);
            slots.Add(temp);
            temp.transform.SetParent(poolGameObject.transform, false);
        }
    }

    void GenerateAbilities()
    {
        for (int x = 0; x < abilitiesToGenerate; ++x)
        {
            UI_Ability temp = Instantiate(abilityPrefab).GetComponent<UI_Ability>();
            abilities.Add(temp.gameObject);
            temp.SetSlot(slots[x], false);
        }
    }
}
