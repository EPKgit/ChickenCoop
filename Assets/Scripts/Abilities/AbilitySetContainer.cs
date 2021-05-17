using System;
using System.Collections;

public enum AbilitySlots
{
    SLOT_ATTACK,
    SLOT_1,
    SLOT_2,
    SLOT_3,
    MAX = 4
}

[System.Serializable]
public struct AbilitySetContainer : IEnumerable
{
    private Ability[] abilities;
    public AbilitySetContainer(params Ability[] list)
    {
        if (list.Length < (int)AbilitySlots.MAX)
        {
            throw new Exception("ERROR: Ability Set passed more abilities than allowed");
        }
        abilities = new Ability[(int)AbilitySlots.MAX];
        for (int x = 0; x < list.Length; ++x)
        {
            abilities[x] = list[x];
        }
    }

    public Ability this[int i]
    {
        get { return abilities[i]; }
        set { abilities[i] = value; }
    }
    public Ability this[AbilitySlots i]
    {
        get { return abilities[(int)i]; }
        set { abilities[(int)i] = value; }
    }

    public float Length
    {
        get { return abilities?.Length ?? -1; }
    }

    public IEnumerator GetEnumerator()
    {
        return abilities.GetEnumerator();
    }
}