using System;
using System.Collections;
using UnityEngine;

public enum AbilitySlot
{
    SLOT_ATTACK,
    SLOT_1,
    SLOT_2,
    SLOT_3,
    MAX = 4,
    DROPPED_ABILITY,
    INVALID,
}

public static class EnumExtensions
{
    public static bool ValidSlot(this AbilitySlot a)
    {
        return a >= AbilitySlot.SLOT_ATTACK && a < AbilitySlot.MAX;
    }
    public static int AsInt(this AbilitySlot a)
    {
        return (int)AbilitySlot.MAX;
    }
}

[System.Serializable]
public struct AbilitySetContainer : IEnumerable
{
    private Ability[] abilities;
    public AbilitySetContainer(params Ability[] list)
    {
        if (list.Length < (int)AbilitySlot.MAX)
        {
            throw new Exception("ERROR: Ability Set passed more abilities than allowed");
        }
        abilities = new Ability[(int)AbilitySlot.MAX];
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
    public Ability this[AbilitySlot i]
    {
        get { return abilities[(int)i]; }
        set { abilities[(int)i] = value; }
    }

    public float Length
    {
        get { return abilities?.Length ?? -1; }
    }

    public bool SetSlot(int i, Ability a)
    {
        if(!((AbilitySlot)i).ValidSlot())
        {
            Debug.LogError("ERROR SETTING SLOT IN ABILITIES INVALID INDEX");
            return false;
        }
        abilities[i] = a;
        return true;
    }

    public bool SetSlot(AbilitySlot i, Ability a)
    {
        return SetSlot((int)i, a);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return abilities.GetEnumerator();
    }

    public AbilitySetContainerEnumerator GetEnumerator()
    {
        return new AbilitySetContainerEnumerator(abilities);
    }
}

public class AbilitySetContainerEnumerator : IEnumerator
{
    private Ability[] _abilities;
    int position = -1;
    public AbilitySetContainerEnumerator(Ability[] a)
    {
        _abilities = a;
    }

    public bool MoveNext()
    {
        position++;
        while (position < _abilities.Length && _abilities[position] == null)
        {
            position++;
        }
        return position < _abilities.Length;
    }

    public void Reset()
    {
        position = -1;
    }

    object IEnumerator.Current
    {
        get
        {
            return Current;
        }
    }

    public Ability Current
    {
        get
        {
            try
            {
                return _abilities[position];
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidOperationException();
            }
        }
    }
}