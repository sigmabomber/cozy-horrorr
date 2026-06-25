using UnityEngine;

public class VegetablePullout : MinigameBase, ITimeBasedMinigame
{
    [Header("Time")]
    [SerializeField] private float duration = 5f;

    [Header("Bar")]
    [SerializeField] private float markerSpeed = 1.5f;
    [SerializeField] private float successZoneSize = 0.18f;
    [SerializeField] private float successZoneCenter = 0.5f;

    private float currentTime;
    private float markerPosition;
    private int direction = 1;

    public float Duration => duration;
    public float CurrentTime => currentTime;
    public float TimeProgress => currentTime / duration;

    public float BarValue => markerPosition;
    public float SuccessZoneCenter => successZoneCenter;
    public float SuccessZoneSize => successZoneSize;

    protected override void OnStartMinigame()
    {
        currentTime = 0f;
        markerPosition = 0f;
        direction = 1;
    }

    protected override void OnMinigameUpdate()
    {
        currentTime += Time.deltaTime;

        markerPosition += direction * markerSpeed * Time.deltaTime;

        if (markerPosition >= 1f)
        {
            markerPosition = 1f;
            direction = -1;
        }
        else if (markerPosition <= 0f)
        {
            markerPosition = 0f;
            direction = 1;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            TryPull();
        }

        if (currentTime >= duration)
        {
            FailMinigame();
        }
    }

    private void TryPull()
    {
        float distance = Mathf.Abs(markerPosition - successZoneCenter);
        float allowedDistance = successZoneSize * 0.5f;

        if (distance <= allowedDistance)
            CompleteMinigame();
        else
            FailMinigame();
    }
}