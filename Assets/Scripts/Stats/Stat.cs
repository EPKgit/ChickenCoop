using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stat : ISerializationCallbackReceiver
{
    /// <summary>
    /// Represents the current value of the stat, given the base value and all of the modifiers. This value
    /// is kept updated as bonuses are added and removed.
    /// </summary>
    /// <value>
    /// Use this value to access the updated current value. Returns the value of _value
    /// </value>
    public float Value
    {
        get
        {
            UpdateCurrentValue();
            return _value;
        }
        private set
        {
            _value = value;
        }
    }
    public int IntValue
    {
        get
        {
            return (int)Value;
        }
    }
    private float _value;

    /// <summary>
    /// The name of the stat that this represents
    /// </summary>
    public StatName name;

	/// <summary>
	/// An event that is fired anytime the stat is changed, either through a new base value
	/// or if a modifier is added or removed
	/// </summary>
	public event Action<float> OnStatChange = delegate {};

	/// <summary>
	/// Our base value, is added to and multiplied by when we calculate our final value
	/// </summary>
    public float BaseValue
    {
        get
        {
            return _baseValue;
        }
        set
        {
            _baseValue = value;
            UpdateCurrentValue(true);
        }
    }
    [SerializeField, HideInInspector]
    private float _baseValue;

	/// <summary>
	/// A marker for the currentID we're using, acts as a handle that's returned to callers
	/// so they can keep track of the bonuses that they've added. Used to remove bonuses
	/// to ensure that they remove the same bonus they added. 
	/// </summary>
    [NonSerialized]
	private uint currentID = 0;

    [NonSerialized]
    private List<Tuple<uint, float>> multiplicativeModifiers = new List<Tuple<uint, float>>();
    [NonSerialized]
    private List<Tuple<uint, float>> additiveModifiers = new List<Tuple<uint, float>>();

	public Stat(StatName s, float f)
	{
		name = s;
		BaseValue = f;
	}

	/// <summary>
	/// Calculates the final value, adding all additiveModifiers, then adding our multiplicativeModifiers
	/// multiplied by the baseValue. Also calls the statChangeEvent
	/// </summary>
    private void UpdateCurrentValue(bool forceUpdate = false)
    {
        float finalResult = BaseValue;
        foreach(Tuple<uint, float> t in additiveModifiers)
        {
            finalResult += t.Item2;
        }
        foreach(Tuple<uint, float> t in multiplicativeModifiers)
        {
            finalResult *= t.Item2;
        }
        bool changed = _value != finalResult;
        _value = finalResult;
        if (changed || forceUpdate)
        {
            OnStatChange(_value);
        }
    }

	uint GetID()
	{
		return currentID++;
	}

	/// <summary>
	/// Adds an additive modifier to the base value
	/// </summary>
	/// <param name="f">The bonus amount</param>
	/// <returns>Returns the ID handle of the bonus added</returns>
    public uint AddAdditiveModifier(float f)
    {
		uint ID = GetID();
        additiveModifiers.Add(new Tuple<uint, float>(ID, f));
        UpdateCurrentValue();
		return ID;
    }

	/// <summary>
	/// Adds a multipicative modifier to the base value
	/// </summary>
	/// <param name="f">The bonus amount</param>
	/// <returns>Returns the ID handle of the bonus added</returns>
    public uint AddMultiplicativeModifier(float f)
    {
		uint ID = GetID();
        multiplicativeModifiers.Add(new Tuple<uint, float>(ID, f));
        UpdateCurrentValue();
        return ID;
    }
    
	/// <summary>
	/// Removes a bonus by it's ID handle
	/// </summary>
	/// <param name="i">The ID of the bonus to remove</param>
    public void RemoveAdditiveModifier(uint i)
    {
        additiveModifiers.RemoveAll( (t) => t.Item1 == i );
        UpdateCurrentValue();
    }

	/// <summary>
	/// Removes a bonus by it's ID handle
	/// </summary>
	/// <param name="i">The ID of the bonus to remove</param>
    public void RemoveMultiplicativeModifier(uint i)
    {
        multiplicativeModifiers.RemoveAll( (t) => t.Item1 == i );
        UpdateCurrentValue();
    }

	/// <summary>
	/// Registers a method to get called when the stat is changed
	/// </summary>
	/// <param name="d">The method to invoke</param>
	public void RegisterStatChangeCallback(Action<float> d)
	{
		OnStatChange += d;
        d(_value);
	}

	/// <summary>
	/// Deregisters a method to get called when the stat is changed
	/// </summary>
	/// <param name="d">The method that was invoked</param>
	public void DeregisterStatChangeCallback(Action<float> d)
	{
		OnStatChange -= d;
	}

    public void OverwriteBaseValueNoUpdate(float f)
    {
        _baseValue = f;
    }

    public override string ToString()
	{
		return string.Format("{0}:{1}", name, Value);
	}

    public void OnBeforeSerialize()
    {
    }

    public void OnAfterDeserialize()
    {
        multiplicativeModifiers = new List<Tuple<uint, float>>();
        additiveModifiers = new List<Tuple<uint, float>>();
        OnStatChange = delegate { };
    }
}
