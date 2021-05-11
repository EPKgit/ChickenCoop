using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayTagComponent : MonoBehaviour
{
    [SerializeField]
    private GameplayTagContainer tags = new GameplayTagContainer();
    public bool Contains(GameplayTagFlags f)
    {
        return tags.Contains(f);
    }

    public int AddTag(GameplayTagFlags f)
    {
        return tags.AddTag(f);
    }

    private bool RemoveFirstTag(GameplayTagFlags f)
    {
        return tags.RemoveFirstTag(f);
    }

    private bool RemoveTagWithID(int ID)
    {
        return tags.RemoveTagWithID(ID);
    }
}
