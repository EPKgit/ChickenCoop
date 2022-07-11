using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatName 
{ 
    Strength, 
    Agility, 
    Toughness, 
    AggroPercentage, 
    DamagePercentage, 
    HealingPercentage, 
    CooldownReduction, 
    PuzzleSolving, 
}

[System.Serializable]
public class StatBlock : ISerializationCallbackReceiver
{
    private List<Tuple<StatName, StatChangeDelegate>> queuedCallbacksToRegister = new List<Tuple<StatName, StatChangeDelegate>>();

    private Dictionary<StatName, Stat> stats = new Dictionary<StatName, Stat>();

    private Dictionary<StatName, float> defaultValues = new Dictionary<StatName, float>()
    {
        { StatName.AggroPercentage,     1.0f },
        { StatName.Agility,             1.0f },
        { StatName.CooldownReduction,   1.0f },
        { StatName.DamagePercentage,    1.0f },
        { StatName.HealingPercentage,   1.0f },
        { StatName.PuzzleSolving,       0.0f },
        { StatName.Strength,            1.0f },
        { StatName.Toughness,           1.0f },
    };

    public void Initialize(StatBlock other)
    {
        var otherStats = other.stats;
        foreach (StatName key in Enum.GetValues(typeof(StatName)))
        {
            if (otherStats.ContainsKey(key)) //if the other stats have a definition for it
            {
                if (HasStat(key)) //override our local value with the other stats value
                {
                    stats[key].BaseValue = otherStats[key].BaseValue;
                }
                else //otherwise add it to our dictionary
                {
                    stats.Add(key, new Stat(key, otherStats[key].BaseValue));
                }
            }
            else //otherwise if it's not defined by the other block
            {
                float defaultValue = defaultValues[key];
                if (HasStat(key)) //make sure we are reset to default
                {
                    stats[key].BaseValue = defaultValue;
                }
                else //add it with default value
                {
                    stats.Add(key, new Stat(key, defaultValue));
                }
            }
        }
        FlushInitializationQueue();
    }
    public void Initialize()
    {
        foreach (StatName key in Enum.GetValues(typeof(StatName)))
        {
            if (!HasStat(key))
            {
                float defaultValue = defaultValues[key];
                stats.Add(key, new Stat(key, defaultValue));
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
            stats[pair.Item1].RegisterStatChangeCallback(pair.Item2);
        }
    }

    /// <summary>
    /// Checks if the StatBlock contains a certain stat
    /// </summary>
    /// <param name="name">The name of the stat to check</param>
    /// <returns>Returns true if the stat is in the block, false if not</returns>
    public bool HasStat(StatName name)
    {
        return stats.ContainsKey(name);
    }

    /// <summary>
    /// Gives us access to the value of a certain stat
    /// </summary>
    /// <param name="name">The name of the stat to check</param>
    /// <returns>Returns the final value of the stat if it exists, -1 if it doesn't</returns>
    public float GetValue(StatName name)
    {
        Stat temp = null;
        stats?.TryGetValue(name, out temp);
        return temp == null ? -1f : temp.Value;
    }

    /// <summary>
    /// Gives us access to a certain stat. Useful for registering callbacks on a stat, or adding bonuses
    /// </summary>
    /// <param name="name">The name of the stat to get</param>
    /// <returns>Returns the stat if it exists, null otherwise</returns>
    public Stat GetStat(StatName name)
    {
        Stat temp = null;
        stats?.TryGetValue(name, out temp);
        return temp;
    }

    public Dictionary<StatName, Stat> GetStats()
    {
        return stats;
    }

    public void RegisterStatChangeCallback(StatName stat, StatChangeDelegate d)
    {
        if (stats.ContainsKey(stat))
        {
            stats[stat].RegisterStatChangeCallback(d);
        }
        else
        {
            queuedCallbacksToRegister.Add(new Tuple<StatName, StatChangeDelegate>(stat, d));
        }
    }

    public void DeregisterStatChangeCallback(StatName stat, StatChangeDelegate d)
    {
        if (stats.ContainsKey(stat))
        {
            stats[stat].DeregisterStatChangeCallback(d);
        }
    }

    [SerializeField, HideInInspector]
    private List<StatName> _keys = new List<StatName>();
    [SerializeField, HideInInspector]
    private List<Stat> _values = new List<Stat>();

    /// <summary>
    /// We have to do manual serialization because unity doesn't support dictionary serialization
    /// </summary>
    public void OnBeforeSerialize()
    {
        _keys.Clear();
        _values.Clear();

        foreach (var kvp in stats)
        {
            _keys.Add(kvp.Key);
            _values.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        stats.Clear();
        for (int x = 0; x < Math.Min(_keys.Count, _values.Count); ++x)
        {
            stats.Add(_keys[x], _values[x]);
        }
    }

    public override string ToString()
    {
        string s = "";
        foreach (var pair in stats)
        {
            s += string.Format("{0}:{1}\n", pair.Key.ToString(), pair.Value.Value);
        }
        return s;
    }
}