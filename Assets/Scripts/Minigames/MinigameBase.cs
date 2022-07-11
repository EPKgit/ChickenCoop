using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Minigame;

namespace Minigame
{
public enum MinigameType
{
    SLIDER,
    MAX
}

public class MinigameData
{
    public int difficultyModifier;
    public PlayerInput triggeringPlayer;
}
}
public abstract class MinigameBase : MonoBehaviour
{
    public enum MinigameStatus
    {
        IN_PROGRESS,
        CANCELLED,
        FINISHED_FAILURE,
        FINISHED_SUCCESS,
        MAX,
    }
    protected enum MinigameInputType
    {
        MOVEMENT,
        INTERACT,
        MOVEMENT_PLUS_INTERACT,
    }
    public abstract MinigameType type { get; }
    protected abstract MinigameInputType inputType { get; }

    protected MinigameData data;

    public void StartMinigame(MinigameData data)
    {
        this.data = data;
        switch (inputType)
        {
            case MinigameInputType.MOVEMENT:
                data.triggeringPlayer.OnMoveEvent += HandleMinigameMoveInputInternal;
                break;
            case MinigameInputType.INTERACT:
                data.triggeringPlayer.OnInteractEvent += HandleMinigameInteractInputInternal;
                break;
            case MinigameInputType.MOVEMENT_PLUS_INTERACT:
                data.triggeringPlayer.OnMoveEvent += HandleMinigameMoveInputInternal;
                data.triggeringPlayer.OnInteractEvent += HandleMinigameInteractInputInternal;
                break;
        }
        StartMinigameInternal();
    }

    protected virtual void StartMinigameInternal() { }

    private void HandleMinigameMoveInputInternal(PlayerInput.InputEventData<Vector2> data)
    {
        data.handled = true;
        HandleMinigameMoveInput(data.data);
    }
    private void HandleMinigameInteractInputInternal(PlayerInput.InputEventData<bool> data)
    {
        data.handled = true;
        HandleMinigameInteractInput(data.data);
    }
    protected virtual void HandleMinigameMoveInput(Vector2 dir) { }
    protected virtual void HandleMinigameInteractInput(bool pressed) { }
    public void FinishMinigame()
    {
        switch (inputType)
        {
            case MinigameInputType.MOVEMENT:
                data.triggeringPlayer.OnMoveEvent -= HandleMinigameMoveInputInternal;
                break;
            case MinigameInputType.INTERACT:
                data.triggeringPlayer.OnInteractEvent -= HandleMinigameInteractInputInternal;
                break;
            case MinigameInputType.MOVEMENT_PLUS_INTERACT:
                data.triggeringPlayer.OnMoveEvent -= HandleMinigameMoveInputInternal;
                data.triggeringPlayer.OnInteractEvent -= HandleMinigameInteractInputInternal;
                break;
        }
        FinishMinigameInternal();
        data = null;
    }
    protected virtual void FinishMinigameInternal() { }
    public abstract MinigameStatus GetMinigameState();

}
