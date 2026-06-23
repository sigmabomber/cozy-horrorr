using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFPSController : MonoBehaviour
{
    [Header("FPS Settings")]
    [Range(1, 240)]
    public int targetFPS = 60;

    private void Awake()
    {
        ApplyFPS();
    }

    private void OnValidate()
    {
        ApplyFPS();
    }

    private void ApplyFPS()
    {
        Application.targetFrameRate = targetFPS;
    }
}