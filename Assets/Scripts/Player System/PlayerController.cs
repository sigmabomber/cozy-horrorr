using System.Collections;
using UnityEngine;
using Doody.GameEvents;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : EventListener
{
    [Header("Mode")]
    [SerializeField] private PlayerMode currentMode = PlayerMode.Garden;

    [Header("Garden Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float turnSpeed = 12f;
    [SerializeField] private Transform cameraTransform;

    [Header("Maze Movement")]
    [SerializeField] private MazeNode currentNode;
    [SerializeField] private float nodeMoveTime = 0.35f;

    private CharacterController controller;
    private bool isMovingNode;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Listen<ChangePlayerModeEvent>(OnChangePlayerMode);
    }

    private void Start()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (currentMode == PlayerMode.Garden)
            UpdateGardenMovement();
        else
            UpdateMazeMovement();
    }

    private void UpdateGardenMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(x, 0f, z).normalized;

        if (input.sqrMagnitude <= 0.01f)
            return;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = camForward * input.z + camRight * input.x;

        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * turnSpeed
        );
    }

    private void UpdateMazeMovement()
    {
        if (isMovingNode || currentNode == null)
            return;

        if (Input.GetKeyDown(KeyCode.W) && currentNode.forward != null)
            StartCoroutine(MoveToNode(currentNode.forward));

        if (Input.GetKeyDown(KeyCode.A) && currentNode.left != null)
            StartCoroutine(MoveToNode(currentNode.left));

        if (Input.GetKeyDown(KeyCode.D) && currentNode.right != null)
            StartCoroutine(MoveToNode(currentNode.right));
    }

    private IEnumerator MoveToNode(MazeNode targetNode)
    {
        isMovingNode = true;

        Vector3 start = transform.position;
        Vector3 end = targetNode.transform.position;

        Quaternion startRot = transform.rotation;
        Quaternion endRot = targetNode.transform.rotation;

        float timer = 0f;

        while (timer < nodeMoveTime)
        {
            timer += Time.deltaTime;
            float t = timer / nodeMoveTime;

            controller.enabled = false;
            transform.position = Vector3.Lerp(start, end, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            controller.enabled = true;

            yield return null;
        }

        currentNode = targetNode;
        transform.position = currentNode.transform.position;
        transform.rotation = currentNode.transform.rotation;

        if (currentNode.cameraPoint != null)
            Events.Publish(new SetMazeCameraNodeEvent(currentNode.cameraPoint));

        isMovingNode = false;
    }

    private void OnChangePlayerMode(ChangePlayerModeEvent e)
    {
        currentMode = e.Mode;

        if (currentMode == PlayerMode.Maze && currentNode != null)
        {
            controller.enabled = false;
            transform.position = currentNode.transform.position;
            transform.rotation = currentNode.transform.rotation;
            controller.enabled = true;

            if (currentNode.cameraPoint != null)
                Events.Publish(new SetMazeCameraNodeEvent(currentNode.cameraPoint));
        }
    }

    public void SetMazeNode(MazeNode node)
    {
        currentNode = node;
    }
}