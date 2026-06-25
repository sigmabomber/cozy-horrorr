using UnityEngine;

public class FishingMinigame : MinigameBase, ITrackingMinigame
{
    [Header("Fish")]
    [SerializeField] private float fishPosition = 0.5f;
    [SerializeField] private float fishSpeed = 1.8f;
    [SerializeField] private float fishChangeInterval = 0.7f;

    [Header("Catch Bar")]
    [SerializeField] private float catchBarPosition = 0.3f;
    [SerializeField] private float catchBarSize = 0.22f;
    [SerializeField] private float liftSpeed = 1.8f;
    [SerializeField] private float gravitySpeed = 1.4f;

    [Header("Progress")]
    [SerializeField] private float catchProgress;
    [SerializeField] private float gainSpeed = 0.35f;
    [SerializeField] private float loseSpeed = 0.25f;

    private float fishDirection;
    private float fishTimer;
    private float barVelocity;

    public float FishPosition => fishPosition;
    public float CatchBarPosition => catchBarPosition;
    public float CatchBarSize => catchBarSize;
    public float CatchProgress => catchProgress;

    protected override void OnStartMinigame()
    {
        fishPosition = 0.5f;
        catchBarPosition = 0.3f;
        catchProgress = 0f;
        barVelocity = 0f;
        PickNewFishDirection();
    }

    protected override void OnMinigameUpdate()
    {
        UpdateFish();
        UpdateCatchBar();
        UpdateProgress();

        if (catchProgress >= 1f)
            CompleteMinigame();
        else if (catchProgress <= 0f)
            FailMinigame();
    }

    private void UpdateFish()
    {
        fishTimer -= Time.deltaTime;

        if (fishTimer <= 0f)
            PickNewFishDirection();

        fishPosition += fishDirection * fishSpeed * Time.deltaTime;

        if (fishPosition <= 0f || fishPosition >= 1f)
        {
            fishPosition = Mathf.Clamp01(fishPosition);
            fishDirection *= -1f;
        }
    }

    private void PickNewFishDirection()
    {
        fishTimer = Random.Range(fishChangeInterval * 0.5f, fishChangeInterval * 1.5f);
        fishDirection = Random.Range(-1f, 1f);
    }

    private void UpdateCatchBar()
    {
        bool holding = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);

        if (holding)
            barVelocity += liftSpeed * Time.deltaTime;
        else
            barVelocity -= gravitySpeed * Time.deltaTime;

        catchBarPosition += barVelocity * Time.deltaTime;
        catchBarPosition = Mathf.Clamp01(catchBarPosition);

        if (catchBarPosition <= 0f || catchBarPosition >= 1f)
            barVelocity = 0f;
    }

    private void UpdateProgress()
    {
        float halfSize = catchBarSize * 0.5f;
        float min = catchBarPosition - halfSize;
        float max = catchBarPosition + halfSize;

        bool fishInsideBar = fishPosition >= min && fishPosition <= max;

        if (fishInsideBar)
            catchProgress += gainSpeed * Time.deltaTime;
        else
            catchProgress -= loseSpeed * Time.deltaTime;

        catchProgress = Mathf.Clamp01(catchProgress);
    }
}