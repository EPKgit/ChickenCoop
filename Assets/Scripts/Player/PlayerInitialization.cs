using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInitialization : MonoBehaviour
{
    public PlayerClass playerClass;

    private PlayerAbilities abilities;
    private StatBlockComponent stats;
    private PlayerInput input;
    void Awake()
    {
        stats = GetComponent<StatBlockComponent>();
        input = GetComponent<PlayerInput>();
        abilities = GetComponent<PlayerAbilities>();
    }

    private void Start()
    {
        stats.Initialize(playerClass.stats);
    }
}
