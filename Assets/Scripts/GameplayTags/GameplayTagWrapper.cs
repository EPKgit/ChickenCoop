using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

using RawGameplayTag = System.String;

namespace GameplayTagInternals
{

[System.Serializable]
public class GameplayTagID
{
    const uint INVALID_ID = uint.MaxValue;
    public uint ID { get => _ID; }
    [SerializeField] private uint _ID = INVALID_ID;

    public bool IsValid()
    {
        return _ID == INVALID_ID;
    }

    static uint counter = 0;
    public void Initialize()
    {
        _ID = counter++;
        if(counter == INVALID_ID)
        {
            counter = 0;
        }
    }

    public static bool operator==(GameplayTagID l, GameplayTagID r)
    {
        if (System.Object.ReferenceEquals(l, r))
        {
            return true;
        }
        if (((object)l == null) || ((object)r == null))
        {
            return false;
        }
        return l.ID == r.ID;
    }

    public static bool operator!=(GameplayTagID l, GameplayTagID r)
    {
        return !(l == r);
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        GameplayTagID p = (GameplayTagID)obj;
        if (p == null)
        {
            return false;
        }
        return p._ID == _ID;
    }

    public override int GetHashCode()
    {
        return _ID.GetHashCode();
    }

    public override string ToString()
    {
        return _ID.ToString();
    }
}

[System.Serializable]
public class GameplayTagWrapper : ISerializationCallbackReceiver
{
    public GameplayTagInternals.GameplayTagID ID;

    public RawGameplayTag Flag 
    { 
        get => _flag; 
        private set
        {
            if(_flag == value)
            {
                return;
            }
            _flag = value;
            RecalculateDelimitedString();
        }
    }
    [SerializeField] private RawGameplayTag _flag;

    public ReadOnlyCollection<RawGameplayTag> DelimitedRawTags 
    { 
        get 
        {
#if UNITY_EDITOR
            if(_delimitedRawTags == null)
            {
                throw new System.Exception();
            }
#endif
            return _delimitedRawTags.AsReadOnly(); 
        }
    }
    private List<RawGameplayTag> _delimitedRawTags;

    /// <summary>
    /// Constructor taking a flag
    /// </summary>
    /// <param name="flag">What tags this defines</param>
    /// <param name="p_internal">Denoting this is not an active tag added and won't get an ID Number</param>
    public GameplayTagWrapper(RawGameplayTag flag, bool p_internal = false)
    {
        this.Flag = flag;
        if (!p_internal)
        {
            ID = new GameplayTagID();
            ID.Initialize();
        }
        
    }

    void RecalculateDelimitedString()
    {
        _delimitedRawTags = new List<RawGameplayTag>();
        var split = _flag.Split('.');
        foreach(var tag in split)
        {
            _delimitedRawTags.Add(tag);
        }
    }

    /// <summary>
    /// Checks if our raw tag string overlaps with another.
    /// This function will check along the strings until our paramater ends and return
    /// false if it detects a mismatch 
    /// for example {A.B.C}.Overlaps({A.B}) returns true however {A.B}.Overlaps({A.B.C}) returns false
    /// </summary>
    /// <param name="other">The other wrapper that we are testing overlap against</param>
    /// <returns>True if the other wrapper has an overlapping tag false otherwise</returns>
    public bool Contains(GameplayTagWrapper other)
    {
        // we can never contain a string that is bigger than ourselves
        if(other._delimitedRawTags.Count > _delimitedRawTags.Count)
        {
            return false;
        }
        for(int x = 0; x < other._delimitedRawTags.Count; ++x)
        {
            if(_delimitedRawTags[x] != other._delimitedRawTags[x])
            {
                return false;
            }
        }
        return true;
    }

    public bool Contains(RawGameplayTag other)
    {
        return Contains(new GameplayTagWrapper(other, true));
    }

    /// <summary>
    /// Checks if our raw tag string perfectly matches another
    /// This function will check for ANY mismatches between the string
    /// for example {A.B.C}.Overlaps({A.B}) returns false {A.B}.Overlaps({A.B}) returns true
    /// </summary>
    /// <param name="other">The other wrapper that we are testing matching against</param>
    /// <returns>True if the other wrapper has exactly the same raw tag as us, false otherwise</returns>
    public bool MatchesExact(GameplayTagWrapper other)
    {
        if(other._delimitedRawTags.Count != _delimitedRawTags.Count)
        {
            return false;
        }
        for(int x = 0; x < other._delimitedRawTags.Count; ++x)
        {
            if(_delimitedRawTags[x] != other._delimitedRawTags[x])
            {
                return false;
            }
        }
        return true;
    }

    public bool MatchesExact(RawGameplayTag other)
    {
        return MatchesExact(new GameplayTagWrapper(other, true));
    }

#region OPERATORS_AND_OVERRIDES

    /// <summary>
    /// Checks if the other tag contains the same flags, up until the point where one of them ends
    /// </summary>
    /// <param name="l"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    public static bool operator==(GameplayTagWrapper l, GameplayTagWrapper r)
    {
        if (System.Object.ReferenceEquals(l, r))
        {
            return true;
        }
        if (((object)l == null) || ((object)r == null))
        {
            return false;
        }
        return l.ID == r.ID;
    }

    public static bool operator !=(GameplayTagWrapper l, GameplayTagWrapper r)
    {
        return !(r == l);
    }


    public static bool operator ==(GameplayTagWrapper l, RawGameplayTag r)
    {
        if (l is null)
        {
            return false;
        }
        return l._flag == r;
    }


    public static bool operator !=(GameplayTagWrapper l, RawGameplayTag r)
    {
        return !(l == r);
    }

    public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            GameplayTagWrapper p = (GameplayTagWrapper)obj;
            return p == this;
        }
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }

    public override string ToString()
    {
        return ID.ToString() + ":" + _flag.ToString();
    }

    public void OnBeforeSerialize()
    {
        
    }

    public void OnAfterDeserialize()
    {
        RecalculateDelimitedString();
    }
#endregion
}
}