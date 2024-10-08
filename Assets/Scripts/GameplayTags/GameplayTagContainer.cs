using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using GameplayTagInternals;

using RawGameplayTag = System.String;

[System.Serializable]
public class GameplayTagContainer
{
    //called before the tag is added in case callsites want to check
    //if the tag was already on the container
    public event Action<GameplayTagWrapper> OnPreTagAdded = delegate {};

    //called after the tag has been removed from the container so we can
    //check if the tag remains from other tags
    public event Action<GameplayTagWrapper> OnPostTagRemoved = delegate {};

    //this is called after the tag is added and before a tag is removed
    public event Action<GameplayTagWrapper> OnTagChanged = delegate {};

    public ReadOnlyCollection<GameplayTagWrapper> Tags
    {
        get => tags.AsReadOnly();
    }
    [SerializeField]
    private List<GameplayTagWrapper> tags = new List<GameplayTagWrapper>();

    /// <summary>
    /// Returns if any of the other containers flags are contained within this container.
    /// Example { A.1, A.1.!, A.2, A.2.@, B.1 }
    /// Contains({A}) is true
    /// Contains({B, C, D} ) is true
    /// Contains({B.1, B.1.!}) is true
    /// Contains({C, D, A.!.@}}) is false
    /// Contains({}) is false
    /// </summary>
    /// <param name="other">Other container to compare to</param>
    /// <returns>True if we have any of their tags, false otherwise</returns>
    public bool ContainsAny(GameplayTagContainer other)
    {
        foreach(var myTag in tags)
        {
            foreach(var otherTag in other.tags)
            {
                if(myTag.Contains(otherTag))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Returns if any of the tags we contain contains the passed in raw tag.
    /// Example { A.1, A.1.!, A.2, A.2.@ }
    /// Contains(A) is true
    /// Contains(A.2) is true
    /// Contains(A.1.!) is true
    /// Contains(A.3) is false
    /// Contains(A.2.!) is false
    /// </summary>
    /// <param name="f">the raw tag to check against</param>
    /// <returns>True if one of our tags contains the parameter tag, false otherwise</returns>
    public bool Contains(RawGameplayTag f)
    {
        foreach(var tag in tags)
        {
            if(tag.Contains(f))
            {
                return true;
            }
        }
        return false;
    }

    public bool Contains(GameplayTagWrapper f)
    {
        return Contains(f.Flag);
    }

    public bool ContainsExact(RawGameplayTag f)
    {
        throw new NotImplementedException();
    }

    public GameplayTagID AddTag(RawGameplayTag f)
    {
        var tag = new GameplayTagWrapper(f);
        OnPreTagAdded(tag);
        tags.Add(tag);
        OnTagChanged(tag);
        return tag.ID;
    }

    public void AddTagPermanent(RawGameplayTag f)
    {
        var tag = new GameplayTagWrapper(f);
        tags.Add(tag);
    }

    public bool RemoveFirstTag(RawGameplayTag f)
    {
        for(int x = 0; x < tags.Count; ++x)
        {
            if(tags[x].MatchesExact(f))
            {
                var toRemove = tags[x];
                OnTagChanged(toRemove);
                tags.RemoveAt(x);
                OnPostTagRemoved(toRemove);
                return true;
            }
        }
        return false;
    }

    public bool RemoveFirstTagWithID(GameplayTagID ID)
    {
        for(int x = 0; x < tags.Count; ++x)
        {
            if(tags[x].ID == ID)
            {
                var toRemove = tags[x];
                OnTagChanged(toRemove);
                tags.RemoveAt(x);
                OnPostTagRemoved(toRemove);
                return true;
            }
        }
        return false;
    }
}