
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(LineRenderer))]
public class NpcAI : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Patrol")]
    public Transform[] patrolPoints;

    [Header("Detection")]
    public float detectionRange = 8f;
    [Range(0f, 360f)]
    public float viewAngle = 120f;

    [Header("Obstacles")]
    public LayerMask obstacleMask;

    [Header("Movement")]
    public float repathInterval = 0.25f;

    [Header("Path Visualization")]
    public float lineHeight = 0.15f;

    private NavMeshAgent agent;
    private LineRenderer lineRenderer;

    private int currentPoint = 0;

    private bool isChasing = false;
    private bool isSearching = false;

    private Vector3 lastSeenPosition;

    private float timer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
    }

    void Start()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[0].position);
        }
    }

    void Update()
    {
        if (target == null) return;

        if (CanSeeTarget())
        {
            ChasePlayer();
        }
        else if (isChasing)
        {
            StartSearching();
        }
        else if (isSearching)
        {
            SearchLastPosition();
        }
        else
        {
            Patrol();
        }

        UpdatePathLine();
    }

    void UpdatePathLine()
    {
        if (agent.pathPending || !agent.hasPath)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        Vector3[] corners = agent.path.corners;

        if (corners.Length < 2)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        lineRenderer.positionCount = corners.Length;

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 point = corners[i];
            point.y += lineHeight;

            lineRenderer.SetPosition(i, point);
        }
    }

    // Oyuncunun görülüp görülmediğini kontrol eder.
    bool CanSeeTarget()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 destination = target.position + Vector3.up * 0.5f;

        Vector3 direction = destination - origin;
        float distance = direction.magnitude;

        if (distance > detectionRange)
            return false;

        float angle = Vector3.Angle(transform.forward, direction);

        if (angle > viewAngle / 2f)
            return false;

        bool blocked = Physics.Linecast(
            origin,
            destination,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        return !blocked;
    }

    // Oyuncuyu takip eder.
    void ChasePlayer()
    {
        isChasing = true;
        isSearching = false;

        lastSeenPosition = target.position;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            agent.SetDestination(lastSeenPosition);
            timer = repathInterval;
        }
    }

    // Oyuncu kaybolduğunda son görülen konuma yönelir.
    void StartSearching()
    {
        isChasing = false;
        isSearching = true;

        agent.SetDestination(lastSeenPosition);
    }

    // Son görülen konuma ulaşmayı kontrol eder.
    void SearchLastPosition()
    {
        if (agent.pathPending)
            return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            isSearching = false;

            GoToNearestPatrolPoint();
        }
    }

    // En yakın devriye noktasını bulur.
    void GoToNearestPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        float closestDistance = Mathf.Infinity;
        int closestIndex = 0;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                patrolPoints[i].position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        currentPoint = closestIndex;

        agent.SetDestination(patrolPoints[currentPoint].position);
    }

    // Devriye sistemi.
    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            currentPoint = (currentPoint + 1) % patrolPoints.Length;

            if (patrolPoints[currentPoint] != null)
            {
                agent.SetDestination(patrolPoints[currentPoint].position);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;

        Vector3 left = Quaternion.Euler(
            0f, -viewAngle / 2f, 0f
        ) * transform.forward;

        Vector3 right = Quaternion.Euler(
            0f, viewAngle / 2f, 0f
        ) * transform.forward;

        Gizmos.DrawRay(transform.position, left * detectionRange);
        Gizmos.DrawRay(transform.position, right * detectionRange);
    }
}
