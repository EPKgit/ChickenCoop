using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatName 
{ 
    MovementSpeed       = 0, 
    MaxHealth           = 1, 
    AggroPercentage     = 2, 
    DamagePercentage    = 3,
    FlatDamage          = 4,
    HealingPercentage   = 5, 
    FlatHealing         = 6, 
    CooldownReduction   = 7, 
    PuzzleSolving       = 8, 
    MAX = 9
}

[System.Serializable]
public class StatBlock : ISerializationCallbackReceiver
{
    private List<Tuple<StatName, Action<float>>> queuedCallbacksToRegister = new List<Tuple<StatName, Action<float>>>();

    private Dictionary<StatName, Stat> statDict = new Dictionary<StatName, Stat>();

    public static Dictionary<StatName, float> defaultValues = new Dictionary<StatName, float>()
    {

        { StatName.MovementSpeed,           1.0f },
        { StatName.MaxHealth,               1.0f },
        { StatName.AggroPercentage,         1.0f },
        { StatName.DamagePercentage,        1.0f },
        { StatName.FlatDamage,              0.0f },
        { StatName.HealingPercentage,       1.0f },
        { StatName.FlatHealing,             0.0f },
        { StatName.CooldownReduction,       1.0f },
        { StatName.PuzzleSolving,           0.0f },
        { StatName.MAX,                     -1.0f },
    };

    public StatBlock()
    {
        foreach (StatName key in Enum.GetValues(typeof(StatName)))
        {
            if (key == StatName.MAX)
            {
                continue;
            }
            float defaultValue = defaultValues[key];
            statDict.Add(key, new Stat(key, defaultValue));
        }
    }

    public void Initialize(StatBlock other)
    {
        var otherStats = other.statDict;
        foreach (StatName key in Enum.GetValues(typeof(StatName)))
        {
            if(key == StatName.MAX)
            {
                continue;
            }
            if (otherStats.ContainsKey(key)) //if the other stats have a definition for it
            {
                if (HasStat(key)) //override our local value with the other stats value
                {
                    statDict[key].BaseValue = otherStats[key].BaseValue;
                }
                else //otherwise add it to our dictionary
                {
                    statDict.Add(key, new Stat(key, otherStats[key].BaseValue));
                }
            }
            else //otherwise if it's not defined by the other block
            {
                float defaultValue = defaultValues[key];
                if (HasStat(key)) //make sure we are reset to default
                {
                    statDict[key].BaseValue = defaultValue;
                }
                else //add it with default value
                {
                    statDict.Add(key, new Stat(key, defaultValue));
                }
            }
        }
        FlushInitializationQueue();
    }
    public void Initialize()
    {
        foreach (StatName key in Enum.GetValues(typeof(StatName)))
        {
            if (key != StatName.MAX && !HasStat(key))
            {
                float defaultValue = defaultValues[key];
                statDict.Add(key, new Stat(key, defaultValue));
            }
        }
        FlushInitializationQueue();
    }

    /// <summary>
    /// We go through all of the callbacks that have been requested before we got initialized and register them
    /// </summary>
    private void FlushInitializationQueue()
    {
        foreach(var pair in queuedCallbacksToRegister)
        {
            statDict[pair.Item1].RegisterStatChangeCallback(pair.Item2);
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
    /// Gives us access to the value of a certain stat
    /// </summary>
    /// <param name="name">The name of the stat to check</param>
    /// <returns>Returns the final value of the stat if it exists, -1 if it doesn't</returns>
    public float GetValue(StatName name)
    {
        Stat temp = null;
        statDict.TryGetValue(name, out temp);
        return temp == null ? -1f : temp.Value;
    }

    public float GetValueOrDefault(StatName name)
    {
        Stat temp = null;
        statDict.TryGetValue(name, out temp);
        return temp == null ? StatBlock.defaultValues[name] : temp.Value;
    }

    public int GetIntValue(StatName name)
    {
        Stat temp = null;
        statDict.TryGetValue(name, out temp);
        return temp == null ? -1 : temp.IntValue;
    }

    public int GetIntValueOrDefault(StatName name)
    {
        Stat temp = null;
        statDict.TryGetValue(name, out temp);
        return temp == null ? (int)StatBlock.defaultValues[name] : temp.IntValue;
    }

    /// <summary>
    /// Gives us access to a certain stat. Useful for registering callbacks on a stat, or adding bonuses
    /// </summary>
    /// <param name="name">The name of the stat to get</param>
    /// <returns>Returns the stat if it exists, null otherwise</returns>
    public Stat GetStat(StatName name)
    {
        Stat temp = null;
        statDict.TryGetValue(name, out temp);
        return temp;
    }

    public Stat AddStat(StatName stat, float value)
    {
        if(statDict.ContainsKey(stat) && stat != StatName.MAX)
        {
            return null;
        }
        Stat newStat = new Stat(stat, value);
        statDict.Add(stat, newStat);
        return newStat;
    }

    public Dictionary<StatName, Stat> GetStats()
    {
        return statDict;
    }

    public bool RegisterStatChangeCallback(StatName stat, Action<float> d)
    {
        if (statDict.ContainsKey(stat))
        {
            statDict[stat].RegisterStatChangeCallback(d);
            return true;
        }
        else
        {
            queuedCallbacksToRegister.Add(new Tuple<StatName, Action<float>>(stat, d));
            return false;
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