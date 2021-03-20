using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInitialization : MonoBehaviour, IStatBlockInitializer
{
    public static List<PlayerInitialization> all = new List<PlayerInitialization>();

    public delegate void PlayerNumberChangedDelegate();
    public static event PlayerNumberChangedDelegate OnPlayerNumberChanged = delegate { };


    public PlayerClass playerClass;
    public int playerID;

    private PlayerAbilities abilities;
    private StatBlockComponent stats;
    private PlayerInput input;

    public StatBlock GetOverridingBlock()
    {
        return playerClass.stats;
    }

    void Awake()
    {
        stats = GetComponent<StatBlockComponent>();
        input = GetComponent<PlayerInput>();
        abilities = GetComponent<PlayerAbilities>();
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
        Initialize();
    }

    public void Initialize()
    {
        GetPlayerID();
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
