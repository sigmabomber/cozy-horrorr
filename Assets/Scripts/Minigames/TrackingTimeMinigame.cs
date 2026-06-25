using UnityEngine;

public abstract class TrackingTimedMinigame : MinigameBase, ITimeBasedMinigame, ITrackingMinigame
{
    [Header("Time")]
    [SerializeField] protected float duration = 20f;

    [Header("Tracking")]
    [SerializeField] protected float targetPosition = 0.5f;
    [SerializeField] protected float playerPosition = 0.5f;
    [SerializeField] protected float playerSpeed = 1.5f;
    [SerializeField] protected float successRange = 0.15f;

    [Header("Progress")]
    [SerializeField] protected float catchProgress;
    [SerializeField] protected float gainSpeed = 0.35f;
    [SerializeField] protected float loseSpeed = 0.2f;

    protected float currentTime;

    public float Duration => duration;
    public float CurrentTime => currentTime;
    public float Progress => currentTime / duration;

    public float TargetPosition => targetPosition;
    public float PlayerPosition => playerPosition;
    public float CatchProgress => catchProgress;

    protected override void OnStartMinigame()
    {
        currentTime = 0f;
        catchProgress = 0f;
        targetPosition = 0.5f;
        playerPosition = 0.5f;
    }

    protected override void OnMinigameUpdate()
    {
        currentTime += Time.deltaTime;

        UpdateTarget();
        UpdatePlayer();
        UpdateCatchProgress();

        if (catchProgress >= 1f)
        {
            CompleteMinigame();
            return;
        }

        if (currentTime >= duration)
        {
            FailMinigame();
        }
    }

    protected virtual void UpdatePlayer()
    {
        float input = 0f;

        if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
            input = 1f;
        else
            input = -1f;

        playerPosition += input * playerSpeed * Time.deltaTime;
        playerPosition = Mathf.Clamp01(playerPosition);
    }

    protected virtual void UpdateCatchProgress()
    {
        bool isOnTarget = Mathf.Abs(playerPosition - targetPosition) <= successRange;

        if (isOnTarget)
            catchProgress += gainSpeed * Time.deltaTime;
        else
            catchProgress -= loseSpeed * Time.deltaTime;

        catchProgress = Mathf.Clamp01(catchProgress);
    }

    protected abstract void UpdateTarget();
}