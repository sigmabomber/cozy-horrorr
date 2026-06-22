using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class DogController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform owner;
    [SerializeField] private Transform foodBowl;
    [SerializeField] private Transform bed;

    [Header("Movement")]
    [SerializeField] private float wanderRadius = 8f;
    [SerializeField] private float followDistance = 5f;
    [SerializeField] private float ownerStopDistance = 2f;

    [Header("Needs")]
    [SerializeField] private float hunger;
    [SerializeField] private float tiredness;
    [SerializeField] private float hungerGainPerSecond = 0.8f;
    [SerializeField] private float tirednessGainPerSecond = 0.5f;
    [SerializeField] private float eatAtHunger = 75f;
    [SerializeField] private float sleepAtTiredness = 85f;

    [Header("Behavior")]
    [SerializeField] private float decisionInterval = 2f;
    [SerializeField] private float barkChance = 0.08f;
    [SerializeField] private float sniffChance = 0.25f;

    [Header("Action Durations")]
    [SerializeField] private float barkDuration = 1.2f;
    [SerializeField] private float eatDuration = 4f;
    [SerializeField] private float sleepDuration = 8f;
    [SerializeField] private float sniffDurationMin = 1.5f;
    [SerializeField] private float sniffDurationMax = 3f;

    private Animator animator;
    private NavMeshAgent agent;
    private bool busy;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int BarkHash = Animator.StringToHash("Bark");
    private static readonly int EatHash = Animator.StringToHash("Eat");
    private static readonly int SleepingHash = Animator.StringToHash("Sleeping");



    [Header("Sounds")]
    [SerializeField] private AudioSource audioSource;

    private float lastBarkSoundTime;

    [SerializeField] private AudioClip barkSound;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        StartCoroutine(BehaviorLoop());
    }

    private void Update()
    {
        hunger += hungerGainPerSecond * Time.deltaTime;
        tiredness += tirednessGainPerSecond * Time.deltaTime;

        hunger = Mathf.Clamp(hunger, 0f, 100f);
        tiredness = Mathf.Clamp(tiredness, 0f, 100f);

        float speed = agent.isOnNavMesh ? agent.velocity.magnitude : 0f;
        animator.SetFloat(SpeedHash, speed);

        if (Input.GetKeyDown(KeyCode.K) && !busy)
        {
            print("ee");
            StartCoroutine(Bark());
        }
    }

    private IEnumerator BehaviorLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(decisionInterval);

            if (busy)
                continue;

            if (!agent.isOnNavMesh)
            {
                Debug.LogWarning("Dog is not on a valid NavMesh.");
                continue;
            }

            if (hunger >= eatAtHunger && foodBowl != null)
            {
                yield return GoEat();
                continue;
            }

            if (tiredness >= sleepAtTiredness && bed != null)
            {
                yield return GoSleep();
                continue;
            }

            if (owner != null)
            {
                float distanceToOwner = Vector3.Distance(transform.position, owner.position);

                if (distanceToOwner > followDistance)
                {
                    MoveNear(owner.position, ownerStopDistance);
                    continue;
                }
            }

            float roll = Random.value;

            if (roll < barkChance)
            {
                yield return Bark();
            }
            else if (roll < barkChance + sniffChance)
            {
                yield return Sniff();
            }
            else
            {
                Wander();
            }
        }
    }

    private IEnumerator GoEat()
    {
        MoveTo(foodBowl.position);
        yield return WaitUntilArrived();

        busy = true;
        StopMoving();

        animator.SetTrigger(EatHash);

        yield return new WaitForSeconds(eatDuration);

        hunger = 0f;
        ResumeMoving();
        busy = false;
    }

    private IEnumerator GoSleep()
    {
        MoveTo(bed.position);
        yield return WaitUntilArrived();

        busy = true;
        StopMoving();

        animator.SetBool(SleepingHash, true);

        yield return new WaitForSeconds(sleepDuration);

        animator.SetBool(SleepingHash, false);

        tiredness = 0f;
        ResumeMoving();
        busy = false;
    }

    private IEnumerator Bark()
    {
        busy = true;
        StopMoving();

        animator.SetTrigger(BarkHash);

        yield return new WaitForSeconds(barkDuration);

        ResumeMoving();
        busy = false;
    }

    public void PlayBarkSound()
    {
        Debug.Log($"BARK EVENT {Time.frameCount}");

        if (audioSource == null || barkSound == null)
            return;

        audioSource.PlayOneShot(barkSound);
    }

    private IEnumerator Sniff()
    {
        busy = true;
        StopMoving();

        float duration = Random.Range(sniffDurationMin, sniffDurationMax);
        float timer = 0f;

        while (timer < duration)
        {
            transform.Rotate(0f, Random.Range(-35f, 35f) * Time.deltaTime, 0f);
            timer += Time.deltaTime;
            yield return null;
        }

        ResumeMoving();
        busy = false;
    }

    private void Wander()
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * wanderRadius;
        MoveNear(randomPoint, wanderRadius);
    }

    private void MoveNear(Vector3 position, float radius)
    {
        if (!agent.isOnNavMesh)
            return;

        if (NavMesh.SamplePosition(position, out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            MoveTo(hit.position);
        }
    }

    private void MoveTo(Vector3 position)
    {
        if (!agent.isOnNavMesh)
            return;

        agent.isStopped = false;
        agent.SetDestination(position);
    }

    private IEnumerator WaitUntilArrived()
    {
        while (agent.isOnNavMesh && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + 0.2f))
        {
            yield return null;
        }
    }

    private void StopMoving()
    {
        if (!agent.isOnNavMesh)
            return;

        agent.ResetPath();
        agent.isStopped = true;
        animator.SetFloat(SpeedHash, 0f);
    }

    private void ResumeMoving()
    {
        if (!agent.isOnNavMesh)
            return;

        agent.isStopped = false;
    }
}