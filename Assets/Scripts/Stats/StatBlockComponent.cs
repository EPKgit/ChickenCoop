﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatBlockComponent : MonoBehaviour
{	
	private StatBlock stats = new StatBlock();

    public void Initialize(StatBlock other)
    {
        stats.Initialize(other);
    }

    public void Initialize(StatBlockComponent other)
    {
        stats.Initialize(other.stats);
    }

    public void Initialize()
	{
        stats.Initialize();
	}
	
	/// <summary>
	/// Checks if the StatBlock contains a certain stat
	/// </summary>
	/// <param name="name">The name of the stat to check</param>
	/// <returns>Returns true if the stat is in the block, false if not</returns>
	public bool HasStat(StatName name)
	{
		return stats.HasStat(name);
	}

	/// <summary>
	/// Gives us access to the value of a certain stat
	/// </summary>
	/// <param name="name">The name of the stat to check</param>
	/// <returns>Returns the final value of the stat if it exists, -1 if it doesn't</returns>
	public float GetValue(StatName name)
	{
		return stats.GetValue(name);
	}

	/// <summary>
	/// Gives us access to a certain stat. Useful for registering callbacks on a stat, or adding bonuses
	/// </summary>
	/// <param name="name">The name of the stat to get</param>
	/// <returns>Returns the stat if it exists, null otherwise</returns>
	public Stat GetStat(StatName name)
	{
        return stats.GetStat(name);
	}

    public StatBlock GetStatBlock()
    {
        return stats;
    }

    public void RegisterStatChangeCallback(StatName stat, StatChangeDelegate d)
	{
        stats.RegisterStatChangeCallback(stat, d);
	}

	public void DeregisterStatChangeCallback(StatName stat, StatChangeDelegate d)
	{
        stats.DeregisterStatChangeCallback(stat, d);
	}
}