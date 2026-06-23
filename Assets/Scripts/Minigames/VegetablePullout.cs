using UnityEngine;

public class VegetablePullout : MinigameBase, ITimeBasedMinigame
{
    [Header("Pull Settings")]
    [SerializeField] private float pullTime = 2f;
    [SerializeField] private bool drainWhenReleased = true;
    [SerializeField] private float drainSpeed = 1f;

    private float currentPullTime;

    public float Duration => pullTime;
    public float CurrentTime => currentPullTime;
    public float Progress => pullTime <= 0f ? 1f : currentPullTime / pullTime;

    protected override void OnStartMinigame()
    {
        currentPullTime = 0f;
    }

    protected override void OnMinigameUpdate()
    {
        if (Input.GetKey(KeyCode.E))
        {
            currentPullTime += Time.deltaTime;
        }
        else if (drainWhenReleased)
        {
            currentPullTime -= Time.deltaTime * drainSpeed;
        }

        currentPullTime = Mathf.Clamp(currentPullTime, 0f, pullTime);

        if (currentPullTime >= pullTime)
        {
            CompleteMinigame();
        }
    }

    protected override void OnCompleteMinigame()
    {
        Debug.Log("Vegetable pulled out!");
    }
}