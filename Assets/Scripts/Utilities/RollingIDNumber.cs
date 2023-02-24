using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingIDNumber
{
    public uint Value
    {
        get
        {
            return _value;
        }
        private set
        {
            _value = value;
            CheckValid();
        }
    }

    private uint _value = uint.MinValue;

    public bool IsValid()
    {
        return _value != uint.MaxValue;
    }

    private void CheckValid()
    {
        if (_value == uint.MaxValue)
        {
            _value = uint.MinValue;
        }
    }

    public static implicit operator uint(RollingIDNumber id) => id._value;
    public static implicit operator int(RollingIDNumber id) => Convert.ToInt32(id._value);
    public static RollingIDNumber operator++(RollingIDNumber id)
    {
        id.Value++;
        return id;
    }
    public static RollingIDNumber operator +(RollingIDNumber id, uint i)
    {
        id.Value += i;
        return id;
    }

    public static RollingIDNumber operator +(RollingIDNumber id, int i)
    {
        id.Value += Convert.ToUInt32(i);
        return id;
    }
}

public abstract class IHandleWithIDNumber
{
    public uint ID { get; protected set; }
    public IHandleWithIDNumber(RollingIDNumber i)
    {
        ID = i;
    }

    private IHandleWithIDNumber() { }
}
