using System;
using UnityEngine;

public abstract class MinigameBase : MonoBehaviour
{
    public event Action<MinigameBase> Started;
    public event Action<MinigameBase> Completed;
    public event Action<MinigameBase> Failed;
    public event Action<MinigameBase> Cancelled;

    [Header("Minigame")]
    [SerializeField] protected string minigameName = "Minigame";
    [SerializeField] protected bool canCancel = true;

    public bool IsRunning { get; private set; }
    public bool IsCompleted { get; private set; }

    public string MinigameName => minigameName;

    public void StartMinigame()
    {
        if (IsRunning) return;

        IsRunning = true;
        IsCompleted = false;

        OnStartMinigame();
        Started?.Invoke(this);
    }

    public void CompleteMinigame()
    {
        if (!IsRunning) return;

        IsRunning = false;
        IsCompleted = true;

        OnCompleteMinigame();
        Completed?.Invoke(this);
    }

    public void FailMinigame()
    {
        if (!IsRunning) return;

        IsRunning = false;
        IsCompleted = false;

        OnFailMinigame();
        Failed?.Invoke(this);
    }

    public void CancelMinigame()
    {
        if (!IsRunning || !canCancel) return;

        IsRunning = false;
        IsCompleted = false;

        OnCancelMinigame();
        Cancelled?.Invoke(this);
    }

    protected virtual void Update()
    {
        if (!IsRunning) return;

        OnMinigameUpdate();
    }

    protected abstract void OnStartMinigame();

    protected virtual void OnMinigameUpdate() { }

    protected virtual void OnCompleteMinigame() { }

    protected virtual void OnFailMinigame() { }

    protected virtual void OnCancelMinigame() { }
}

public interface ITimeBasedMinigame
{
    float Duration { get; }
    float CurrentTime { get; }
    float TimeProgress { get; }

    float BarValue { get; }
    float SuccessZoneCenter { get; }
    float SuccessZoneSize { get; }
}

public interface ITrackingMinigame
{
    float FishPosition { get; }
    float CatchBarPosition { get; }
    float CatchBarSize { get; }
    float CatchProgress { get; }
}