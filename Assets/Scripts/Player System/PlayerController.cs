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

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private CharacterController controller;
    private bool isMovingNode;

    private static readonly int RunHash = Animator.StringToHash("Run");

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        Listen<ChangePlayerModeEvent>(OnChangePlayerMode);
    }

    private void Start()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        SetRunAnimation(false);
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
        if (controller == null || !controller.enabled || !gameObject.activeInHierarchy)
            return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(x, 0f, z).normalized;
        bool isRunning = input.sqrMagnitude > 0.01f;

        SetRunAnimation(isRunning);

        if (!isRunning)
            return;

        if (cameraTransform == null)
            return;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = camForward * input.z + camRight * input.x;

        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * turnSpeed
            );
        }
    }

    private void UpdateMazeMovement()
    {
        if (!isMovingNode)
            SetRunAnimation(false);

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
        SetRunAnimation(true);

        Vector3 start = transform.position;
        Vector3 end = targetNode.transform.position;

        Quaternion startRot = transform.rotation;
        Quaternion endRot = targetNode.transform.rotation;

        float timer = 0f;

        if (controller != null)
            controller.enabled = false;

        while (timer < nodeMoveTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / nodeMoveTime);

            transform.position = Vector3.Lerp(start, end, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        transform.position = end;
        transform.rotation = endRot;

        currentNode = targetNode;

        if (controller != null)
            controller.enabled = true;

        if (currentNode.cameraPoint != null)
            Events.Publish(new SetMazeCameraNodeEvent(currentNode.cameraPoint));

        SetRunAnimation(false);
        isMovingNode = false;
    }

    private void OnChangePlayerMode(ChangePlayerModeEvent e)
    {
        currentMode = e.Mode;
        SetRunAnimation(false);

        if (currentMode == PlayerMode.Maze && currentNode != null)
        {
            if (controller != null)
                controller.enabled = false;

            transform.position = currentNode.transform.position;
            transform.rotation = currentNode.transform.rotation;

            if (controller != null)
                controller.enabled = true;

            if (currentNode.cameraPoint != null)
                Events.Publish(new SetMazeCameraNodeEvent(currentNode.cameraPoint));
        }
    }

    private void SetRunAnimation(bool running)
    {
        if (animator == null)
            return;

        animator.SetBool(RunHash, running);
    }

    public void SetMazeNode(MazeNode node)
    {
        currentNode = node;
    }
}