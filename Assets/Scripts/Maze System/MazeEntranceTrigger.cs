using UnityEngine;
using Doody.GameEvents;

public class MazeEntranceTrigger : MonoBehaviour
{
    [SerializeField] private MazeNode firstNode;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
            player.SetMazeNode(firstNode);

        Events.Publish(new ChangeCameraModeEvent(GameCameraMode.Maze));
        Events.Publish(new ChangePlayerModeEvent(PlayerMode.Maze));

        if (firstNode.cameraPoint != null)
            Events.Publish(new SetMazeCameraNodeEvent(firstNode.cameraPoint));
    }
}