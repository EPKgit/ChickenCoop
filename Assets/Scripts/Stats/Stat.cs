using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stat
{
	/// <summary>
	/// Represents the current value of the stat, given the base value and all of the modifiers. This value
	/// is kept updated as bonuses are added and removed.
	/// </summary>
	/// <value>
	/// Use this value to access the updated current value. Returns the value of _value
	/// </value>
    public float value
	{
		get
		{
			return _value;
		}
	}
	private float _value;

	/// <summary>
	/// The name of the stat that this represents
	/// </summary>
	public StatName name;

	public delegate void StatChangeDelegate(float value);
	/// <summary>
	/// An event that is fired anytime the stat is changed, either through a new base value
	/// or if a modifier is added or removed
	/// </summary>
	public event StatChangeDelegate statChangeEvent = delegate {};

	/// <summary>
	/// Our base value, is added to and multiplied by when we calculate our final value
	/// </summary>
    private float baseValue;

	/// <summary>
	/// A marker for the currentID we're using, acts as a handle that's returned to callers
	/// so they can keep track of the bonuses that they've added. Used to remove bonuses
	/// to ensure that they remove the same bonus they added. 
	/// </summary>
	private int currentID = 0;

    private List<Tuple<float, int>> multiplicativeModifiers = new List<Tuple<float, int>>();
    private List<Tuple<float, int>> additiveModifiers = new List<Tuple<float, int>>();

	public Stat(StatName s, float f)
	{
		name = s;
		SetBaseValue(f);
	}

	public Stat(StatInspectorValue s)
	{
		name = s.name;
		SetBaseValue(s.value);
		if(DEBUGFLAGS.STATS) Debug.Log(name + " created with value:" + baseValue);
	}

	/// <summary>
	/// Calculates the final value, adding all additiveModifiers, then adding our multiplicativeModifiers
	/// multiplied by the baseValue. Also calls the statChangeEvent
	/// </summary>
    private void UpdateCurrentValue()
    {
        float finalResult = baseValue;
        foreach(Tuple<float, int> t in additiveModifiers)
        {
            finalResult += t.Item1;
        }
        foreach(Tuple<float, int> t in multiplicativeModifiers)
        {
            finalResult += t.Item1 * baseValue;
        }
        _value = finalResult;
		statChangeEvent(_value);
    }

    public void SetBaseValue(float f)
    {
        baseValue = f;
        UpdateCurrentValue();
    }

	int GetID()
	{
		return currentID++;
	}

	/// <summary>
	/// Adds an additive modifier to the base value
	/// </summary>
	/// <param name="f">The bonus amount</param>
	/// <returns>Returns the ID handle of the bonus added</returns>
    public int AddAdditiveModifier(float f)
    {
		int ID = GetID();
        additiveModifiers.Add(new Tuple<float, int>(f, ID));
		UpdateCurrentValue();
		return ID;
    }

	/// <summary>
	/// Adds a multipicative modifier to the base value
	/// </summary>
	/// <param name="f">The bonus amount</param>
	/// <returns>Returns the ID handle of the bonus added</returns>
    public int AddMultiplicativeModifier(float f)
    {
		int ID = GetID();
        multiplicativeModifiers.Add(new Tuple<float, int>(f, ID));
        UpdateCurrentValue();
		return ID;
    }
    
	/// <summary>
	/// Removes a bonus by it's ID handle
	/// </summary>
	/// <param name="i">The ID of the bonus to remove</param>
    public void RemoveAdditiveModifier(int i)
    {
        additiveModifiers.RemoveAll( (t) => t.Item2 == i );
        UpdateCurrentValue();
    }

	/// <summary>
	/// Removes a bonus by it's ID handle
	/// </summary>
	/// <param name="i">The ID of the bonus to remove</param>
    public void RemoveMultiplicativeModifier(int i)
    {
        multiplicativeModifiers.RemoveAll( (t) => t.Item2 == i );
        UpdateCurrentValue();
    }

	/// <summary>
	/// Registers a method to get called when the stat is changed
	/// </summary>
	/// <param name="d">The method to invoke</param>
	public void RegisterStatChangeCallback(StatChangeDelegate d)
	{
		statChangeEvent += d;
	}

	/// <summary>
	/// Deregisters a method to get called when the stat is changed
	/// </summary>
	/// <param name="d">The method that was invoked</param>
	public void UnregisterStatChangeCallback(StatChangeDelegate d)
	{
		statChangeEvent -= d;
	}


	public override string ToString()
	{
		return string.Format("{0}:{1}", name, _value);
	}
}
