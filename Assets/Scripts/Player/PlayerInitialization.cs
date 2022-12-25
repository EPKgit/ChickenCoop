using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInitialization : MonoBehaviour, IStatBlockInitializer
{
    public static PlayerInitialization LocalPlayer = null;
    public static List<PlayerInitialization> all = new List<PlayerInitialization>();

    public static event Action OnPlayerNumberChanged = delegate { };


    public PlayerClass playerClass;
    [System.NonSerialized, HideInInspector]
    public int playerID = -1;

    private PlayerAbilities abilities;
    private StatBlockComponent stats;
    private PlayerInput input;

    public StatBlock GetOverridingBlock()
    {
        return playerClass?.stats;
    }

    void Awake()
    {
        stats = GetComponent<StatBlockComponent>();
        input = GetComponent<PlayerInput>();
        abilities = GetComponent<PlayerAbilities>();
        if (/*Placeholder in case of multiplayer*/ true)
        {
            LocalPlayer = this;
        }
        all.Add(this);
        OnPlayerNumberChanged.Invoke();
    }

    void OnDisable()
    {
        all.Remove(this);
        OnPlayerNumberChanged.Invoke();
    }

    private void Start()
    {
        stats.Initialize(playerClass.stats);
        abilities.Initialize(playerClass);
        GetPlayerID();
        input.playerID = playerID;
        gameObject.name = "Player " + playerID;
    }



    void GetPlayerID()
    {
        if (playerID != -1)
        {
            return;
        }
        int max = 0;
        for (int x = 0; x < all.Count; ++x)
        {
            max = Mathf.Max(max, all[x].playerID);
        }
        playerID = max + 1;
    }
}
