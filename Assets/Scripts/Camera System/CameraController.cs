using UnityEngine;
using Doody.GameEvents;

public class CameraController : EventListener
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Garden Camera")]
    [SerializeField] private Vector3 gardenOffset = new Vector3(0f, 8f, -7f);
    [SerializeField] private Vector3 gardenRotation = new Vector3(45f, 0f, 0f);
    [SerializeField] private float followSmooth = 8f;
    [SerializeField] private float rotationSmooth = 8f;

    [Header("Maze Camera")]
    [SerializeField] private float mazeMoveSmooth = 10f;
    [SerializeField] private float mazeRotationSmooth = 10f;

    private GameCameraMode currentMode = GameCameraMode.Garden;
    private Transform currentMazeCameraPoint;

    private void Awake()
    {
        Listen<ChangeCameraModeEvent>(OnChangeCameraMode);
        Listen<SetMazeCameraNodeEvent>(OnSetMazeCameraNode);
    }

    private void LateUpdate()
    {
        if (currentMode == GameCameraMode.Garden)
            UpdateGardenCamera();
        else
            UpdateMazeCamera();
    }

    private void UpdateGardenCamera()
    {
        if (player == null) return;

        Vector3 targetPosition = player.position + gardenOffset;
        Quaternion targetRotation = Quaternion.Euler(gardenRotation);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * followSmooth
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSmooth
        );
    }

    private void UpdateMazeCamera()
    {
        if (currentMazeCameraPoint == null) return;

        transform.position = Vector3.Lerp(
            transform.position,
            currentMazeCameraPoint.position,
            Time.deltaTime * mazeMoveSmooth
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            currentMazeCameraPoint.rotation,
            Time.deltaTime * mazeRotationSmooth
        );
    }

    private void OnChangeCameraMode(ChangeCameraModeEvent e)
    {
        currentMode = e.Mode;
    }

    private void OnSetMazeCameraNode(SetMazeCameraNodeEvent e)
    {
        currentMazeCameraPoint = e.CameraPoint;
        currentMode = GameCameraMode.Maze;
    }
}