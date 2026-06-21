using UnityEngine;

public enum GameCameraMode
{
    Garden,
    Maze
}

public struct ChangeCameraModeEvent
{
    public GameCameraMode Mode;

    public ChangeCameraModeEvent(GameCameraMode mode)
    {
        Mode = mode;
    }
}

public struct SetMazeCameraNodeEvent
{
    public Transform CameraPoint;

    public SetMazeCameraNodeEvent(Transform cameraPoint)
    {
        CameraPoint = cameraPoint;
    }
}