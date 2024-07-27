using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum StatName 
{ 
    //General 
    MovementSpeed, 
    MaxHealth,
    
    //Damage
    AggroPercentage, 
    DamagePercentage,
    FlatDamage,

    //Healing
    HealingPercentage, 
    FlatHealing, 

    //Shielding
    ShieldingPercentage,
    FlatShielding,

    //Knockback
    KnockbackDurationResistance,
    KnockbackForceResistance,

    //Abilities
    CooldownReduction, 
    CooldownSpeed, 
    CastingSpeed,
    SpeedWhileCastingModifier,

    //Misc
    PuzzleSolving, 

    // PORCUPINE
    SpineDuration,
    SpineSpeed, 

    MAX
}

[System.Serializable]
public class StatBlock : ISerializationCallbackReceiver
{
    private List<Tuple<StatName, Action<float>>> queuedCallbacksToRegister = new List<Tuple<StatName, Action<float>>>();

    private Dictionary<StatName, Stat> statDict = new Dictionary<StatName, Stat>();

    public static Dictionary<StatName, float> defaultValues = new Dictionary<StatName, float>()
    {

        { StatName.MovementSpeed,               1.0f },
        { StatName.MaxHealth,                   1.0f },
        { StatName.AggroPercentage,             1.0f },
        { StatName.DamagePercentage,            1.0f },
        { StatName.FlatDamage,                  0.0f },
        { StatName.HealingPercentage,           1.0f },
        { StatName.FlatHealing,                 0.0f },
        { StatName.ShieldingPercentage,         1.0f },
        { StatName.FlatShielding,               0.0f },
        { StatName.CooldownReduction,           1.0f },
        { StatName.CooldownSpeed,               1.0f },
        { StatName.CastingSpeed,                1.0f },
        { StatName.SpeedWhileCastingModifier,   0.2f },
        { StatName.KnockbackDurationResistance, 0.0f },
        { StatName.KnockbackForceResistance,    0.0f },
        { StatName.PuzzleSolving,               0.0f },
        { StatName.SpineDuration,               0.2f },
        { StatName.SpineSpeed,                  25.0f },
    };

    static StatBlock()
    {
        if(defaultValues.Count != (int)StatName.MAX)
        {
            Debug.LogError($"ERROR: not all stats have default values {defaultValues} != {(int)StatName.MAX}");
        }
    }


    private bool initialized = false;
    public StatBlock() 
    {
        initialized = false;
    }

    public void Initialize(StatBlock other)
    {
        var otherStats = other.statDict;
        foreach(var statPair in otherStats) 
        {
            StatName statName = statPair.Key;
            if (statName == StatName.MAX)
            {
                continue;
            }
            Stat theirStat = statPair.Value;
            Stat ourStat = GetStat(statName);
            ourStat.BaseValue = theirStat.BaseValue;
        }
        FlushInitializationQueue();
        initialized = true;
    }
    public void Initialize()
    {
        FlushInitializationQueue();
        initialized = true;
    }

    /// <summary>
    /// We go through all of the callbacks that have been requested before we got initialized and register them
    /// </summary>
    private void FlushInitializationQueue()
    {
        foreach(var pair in queuedCallbacksToRegister)
        {
            GetStat(pair.Item1).RegisterStatChangeCallback(pair.Item2);
        }
    }

    /// <summary>
    /// Checks if the StatBlock contains a certain stat
    /// </summary>
    /// <param name="name">The name of the stat to check</param>
    /// <returns>Returns true if the stat is in the block, false if not</returns>
    public bool HasStat(StatName name)
    {
        return statDict.ContainsKey(name);
    }

    /// <summary>
    /// Gives us access to the value of a certain stat and creates it with a default value if it doesn't exist
    /// </summary>
    /// <param name="name">The name of the stat to check</param>
    /// <returns>Returns the final value of the stat</returns>
    public float GetValue(StatName name)
    {
        return GetStat(name).Value;
    }

    /// <summary>
    /// Gives us access to the value of a certain stat as an integer and creates it with a default value if it doesn't exist
    /// </summary>
    /// <param name="name">The name of the stat to check</param>
    /// <returns>Returns the final value of the stat as an integer</returns>
    public int GetIntValue(StatName name)
    {
        return GetStat(name).IntValue;
    }

    /// <summary>
    /// Gives us access to a certain stat. Useful for registering callbacks on a stat, or adding bonuses
    /// </summary>
    /// <param name="name">The name of the stat to get</param>
    /// <returns>Returns the stat if it exists, otherwise we create it with a default value then return that</returns>
    public Stat GetStat(StatName name)
    {
        Stat temp;
        statDict.TryGetValue(name, out temp);
        if(temp != null)
        {
            return temp;
        }
        return AddStat(name, defaultValues[name]);
    }

    public Stat AddStat(StatName stat, float value)
    {
        if(statDict.ContainsKey(stat) || stat >= StatName.MAX || stat < 0)
        {
            throw new Exception("ERROR: attemping to create duplicate value or invalid index ~ " + stat);
        }
        Stat newStat = new Stat(stat, value);
        statDict.Add(stat, newStat);
        return newStat;
    }

    public Dictionary<StatName, Stat> GetStats()
    {
        return statDict;
    }

    public bool RegisterStatChangeCallback(StatName statName, Action<float> d)
    {
        if(!initialized)
        {
            queuedCallbacksToRegister.Add(new Tuple<StatName, Action<float>>(statName, d));
            return false;
        }
        else
        {
            GetStat(statName).RegisterStatChangeCallback(d);
            return true;
        }
    }

    public bool DeregisterStatChangeCallback(StatName stat, Action<float> d)
    {
        if (statDict.ContainsKey(stat))
        {
            statDict[stat].DeregisterStatChangeCallback(d);
            return true;
        }
        return false;
    }

    [SerializeField, HideInInspector]
    private StatName[] _keys;
    [SerializeField, HideInInspector]
    private Stat[] _values;
    [SerializeField, HideInInspector]
    private bool serializationOverriden = false;

    /// <summary>
    /// We have to do manual serialization because unity doesn't support dictionary serialization
    /// </summary>
    public void OnBeforeSerialize()
    {
        if(serializationOverriden)
        {
            serializationOverriden = false;
            OnAfterDeserialize();
        }
        else
        {
            _keys = new StatName[statDict.Count];
            _values = new Stat[statDict.Count];
        }

        int x = 0;
        foreach (var kvp in statDict)
        {
            _keys[x] = kvp.Key;
            _values[x++] = kvp.Value;
        }
    }

    public void OnAfterDeserialize()
    {
        statDict = new Dictionary<StatName, Stat>();
        for (int x = 0; x < Math.Min(_keys.Length, _values.Length); ++x)
        {
            statDict.Add(_keys[x], _values[x]);
        }
    }

    public override string ToString()
    {
        string s = "";
        foreach (var pair in statDict)
        {
            s += string.Format("{0}:{1}\n", pair.Key.ToString(), pair.Value.Value);
        }
        return s;
    }
}