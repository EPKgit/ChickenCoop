using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Minigame;

public class MinigameManager : MonoSingleton<MinigameManager>
{
    public GameObject[] minigamePrefabs;

    private MinigameBase currentMinigame;
    private Action<MinigameBase.MinigameStatus> callback;
    private GameObject currentPlayer;

    public bool StartMinigame(MinigameType type, Action<MinigameBase.MinigameStatus> cb, GameObject starter)
    {
        if(currentMinigame != null || (int)type > (int)MinigameType.MAX)
        {
            return false;
        }
        if(type == MinigameType.MAX)
        {
            type = (MinigameType)UnityEngine.Random.Range(0, (int)MinigameType.MAX);
        }
        GameObject g = Instantiate(minigamePrefabs[(int)type]);
        g.transform.SetParent(GameObject.Find("ScreenToWorldCanvas").transform);
        currentPlayer = starter;
        g.transform.position = Camera.main.WorldToScreenPoint(currentPlayer.transform.position);
        currentMinigame = Lib.FindDownwardsInTree<MinigameBase>(g);
        MinigameData data = new MinigameData();
        data.triggeringPlayer = starter.GetComponent<PlayerInput>();
        data.difficultyModifier = Lib.LibGetComponentDownTree<StatBlockComponent>(starter).GetIntValueOrDefault(StatName.PuzzleSolving);
        currentMinigame.StartMinigame(data);
        callback = cb;
        return true;
    }

    void Update()
    {
        if(currentMinigame == null)
        {
            return;
        }
        var state = currentMinigame.GetMinigameState();
        if(state != MinigameBase.MinigameStatus.IN_PROGRESS)
        {
            FinishMinigame(state);
            return;
        }
        currentMinigame.transform.position = Camera.main.WorldToScreenPoint(currentPlayer.transform.position);
    }

    public void ForceEndMinigame()
    {
        FinishMinigame(MinigameBase.MinigameStatus.CANCELLED);
    }

    void FinishMinigame(MinigameBase.MinigameStatus status)
    {
        if(currentMinigame == null)
        {
            return;
        }
        currentMinigame.FinishMinigame();
        callback(status);
        Destroy(currentMinigame.gameObject);
        currentMinigame = null;
        callback = null;
        currentPlayer = null;
    }
}
