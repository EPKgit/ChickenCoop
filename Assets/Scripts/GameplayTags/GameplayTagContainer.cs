using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameplayTagConstants
{
    public const int LAYER_OFFSET = 0;
    public const int LAYER_MASK = 0x00000003;
    public const int LAYER_1 = 0;
    public const int LAYER_2 = 1;
    public const int LAYER_3 = 2;
    public const int LAYER_4 = 3;
    public const int LAYER_1_BIT_OFFSET = 2;
    public const uint LAYER_1_BIT_MASK = 0x000000fC;
    public const int LAYER_2_BIT_OFFSET = 8;
    public const uint LAYER_2_BIT_MASK = 0x0000ff00;
    public const int LAYER_3_BIT_OFFSET = 16;
    public const uint LAYER_3_BIT_MASK = 0x00ff0000;
    public const int LAYER_4_BIT_OFFSET = 24;
    public const uint LAYER_4_BIT_MASK = 0xff000000;
    public const int NUM_LAYERS = 4;
}


[Flags]
public enum GameplayTagFlags : UInt32
{
    NONE = 0,
    MOVEMENT = (1 << 0 + GameplayTagConstants.LAYER_1_BIT_OFFSET) | GameplayTagConstants.LAYER_1,
        NORMAL_MOVEMENT_DISABLED = (MOVEMENT) | (1 << 0 + GameplayTagConstants.LAYER_2_BIT_OFFSET) | GameplayTagConstants.LAYER_2,
        MOVEMENT_DASHING = (MOVEMENT) | (1 << 1 + GameplayTagConstants.LAYER_2_BIT_OFFSET) | GameplayTagConstants.LAYER_2,
    INTERACTION  = (1 << 1 + GameplayTagConstants.LAYER_1_BIT_OFFSET) | GameplayTagConstants.LAYER_1,
    ABILITY     = (1 << 2 + GameplayTagConstants.LAYER_1_BIT_OFFSET) | GameplayTagConstants.LAYER_1,
        ABILITY_MOVEMENT = (ABILITY) | (1 << 0 + GameplayTagConstants.LAYER_2_BIT_OFFSET) | GameplayTagConstants.LAYER_2,
    STATUS      = (1 << 3 + GameplayTagConstants.LAYER_1_BIT_OFFSET) | GameplayTagConstants.LAYER_1,
}

[System.Serializable]
public class GameplayTag
{
    public int ID
    {
        get
        {
            return _ID;
        }
    }
    [SerializeField]
    private int _ID = -1;

    public GameplayTagFlags Flag
    {
        get
        {
            return _flag;
        }
    }
    [SerializeField]
    private GameplayTagFlags _flag;

    private static int[] offset = { GameplayTagConstants.LAYER_1_BIT_OFFSET, GameplayTagConstants.LAYER_2_BIT_OFFSET, GameplayTagConstants.LAYER_3_BIT_OFFSET, GameplayTagConstants.LAYER_4_BIT_OFFSET };
    private static uint[] masks = { GameplayTagConstants.LAYER_1_BIT_MASK, GameplayTagConstants.LAYER_2_BIT_MASK, GameplayTagConstants.LAYER_3_BIT_MASK, GameplayTagConstants.LAYER_4_BIT_MASK };

    /// <summary>
    /// Constructor taking a flag
    /// </summary>
    /// <param name="flag">What tags this defines</param>
    /// <param name="p_internal">Denoting this is not an active tag added and won't get an ID Number</param>
    public GameplayTag(GameplayTagFlags flag, bool p_internal = false)
    {
        _flag = flag;
        if (!p_internal)
        {
            _ID = GenerateIDNumber();
        }
    }

    /// <summary>
    /// Constructor taking an int to be cast to a flag
    /// </summary>
    /// <param name="flag">What tags this defines</param>
    /// <param name="p_internal">Denoting this is not an active tag added and won't get an ID Number</param>
    public GameplayTag(int flag, bool p_internal = false) : this((GameplayTagFlags) flag, p_internal)
    {
    }

    static int counter = 0;
    private int GenerateIDNumber()
    {
        return counter++;
    }

    /// <summary>
    /// Checks if the other tag contains the same flags, up until the point where one of them ends
    /// </summary>
    /// <param name="l"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    public static bool operator==(GameplayTag l, GameplayTag r)
    {
        if (l is null)
        {
            return r is null;
        }
        for (int x = 0; x < GameplayTagConstants.NUM_LAYERS; ++x)
        {
            if ((masks[x] & (UInt32)l._flag) == 0 || (masks[x] & (UInt32)r._flag) == 0)
            {
                break;
            }
            if ((masks[x] & (UInt32)l._flag) != (masks[x] & (UInt32)r._flag))
            {
                return false;
            }
        }
        return true;
    }

    public static bool operator !=(GameplayTag l, GameplayTag r)
    {
        return !(r == l);
    }


    public static bool operator ==(GameplayTag l, GameplayTagFlags r)
    {
        if (l is null)
        {
            return false;
        }
        for (int x = 0; x < GameplayTagConstants.NUM_LAYERS; ++x)
        {
            if ((masks[x] & (UInt32)l._flag) == 0 || (masks[x] & (UInt32)r) == 0)
            {
                break;
            }
            if ((masks[x] & (UInt32)l._flag) != (masks[x] & (UInt32)r))
            {
                return false;
            }
        }
        return true;
    }


    public static bool operator !=(GameplayTag l, GameplayTagFlags r)
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
            GameplayTag p = (GameplayTag)obj;
            return p == this;
        }
    }

    public override int GetHashCode()
    {
        return _flag.GetHashCode();
    }

    public override string ToString()
    {
        return ID.ToString() + ":" + _flag.ToString();
    }

}

[System.Serializable]
public class GameplayTagContainer
{
    [SerializeField]
    private List<GameplayTag> tags;

    /// <summary>
    /// Returns if any of the other containers flags are contained within this container
    /// </summary>
    /// <param name="other">Other container to compare to</param>
    /// <returns>True if we have any of their tags, false otherwise</returns>
    public bool Matches(GameplayTagContainer other)
    {
        foreach(var tag in other.tags)
        {
            if(Contains(tag.Flag))
            {
                return true;
            }
        }
        return false;
    }

    public bool Contains(GameplayTagFlags f)
    {
        foreach (var t in tags)
        {
            if (t == f)
            {
                return true;
            }
        }
        return false;
    }

    public bool ContainsExact(GameplayTagFlags f)
    {
        foreach(var t in tags)
        {
            if(t.Flag == f)
            {
                return true;
            }
        }
        return false;
    }

    public int AddTag(GameplayTagFlags f)
    {
        var tag = new GameplayTag(f);
        tags.Add(tag);
        return tag.ID;
    }

    public bool RemoveFirstTag(GameplayTagFlags f)
    {
        foreach (var t in tags)
        {
            if (t == f)
            {
                tags.Remove(t);
                return true;
            }
        }
        return false;
    }

    public bool RemoveTagWithID(int ID)
    {
        foreach (var t in tags)
        {
            if (t.ID == ID)
            {
                return tags.Remove(t);
            }
        }
        return false;
    }

    public IList<GameplayTag> GetGameplayTags()
    {
        return tags.AsReadOnly();
    }
}
