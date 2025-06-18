using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent), typeof(AnimationControl), typeof(IDamageable))]
public class EnemyController : MonoBehaviour
{
    private enum AIState { Patrolling, Chasing, Attacking }
    private AIState currentState;

    [Header("Referências")]
    private NavMeshAgent agent;
    private AnimationControl animControl;
    private Player player;
    private IDamageable selfDamageable;

    [Header("Comportamento de Ataque")]
    [SerializeField] private bool stopsToAttack = true;
    [SerializeField] private float attackCooldown = 2.0f;
    private float lastAttackTime = -999f;

    [Header("Patrulha Aleatória")]
    [SerializeField] private bool canPatrol = true;
    [SerializeField] private float patrolRadius = 7f;
    [SerializeField] private Vector2 patrolWaitTimeRange = new Vector2(3f, 6f);
    private Vector3 startPosition;
    private float patrolWaitTimer;

    [Header("Visão e Perseguição")]
    [SerializeField] private float visionRange = 8f;
    [SerializeField] private float visionAngle = 90f;
    [SerializeField] private LayerMask visionObstacleMask;
    [SerializeField] private float persistenceDuration = 3f;
    private float timeSinceLostSight;

    private int currentAnimState = -1;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animControl = GetComponent<AnimationControl>();
        selfDamageable = GetComponent<IDamageable>();
        player = FindObjectOfType<Player>();
    }

    private void Start()
    {
        currentState = AIState.Patrolling;
        startPosition = transform.position;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        SetAnimationState(0);
    }

    private void Update()
    {
        if (player == null || selfDamageable.IsDead())
        {
            if (agent.enabled) agent.isStopped = true;
            return;
        }

        switch (currentState)
        {
            case AIState.Patrolling: HandlePatrollingState(); break;
            case AIState.Chasing: HandleChasingState(); break;
            case AIState.Attacking: HandleAttackingState(); break;
        }
    }

    private void SetAnimationState(int state)
    {
        if (currentAnimState == state) return;

        animControl.PlayAnim(state);
        currentAnimState = state;
    }

    private void HandlePatrollingState()
    {
        agent.isStopped = false;
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SetAnimationState(0);
            patrolWaitTimer -= Time.deltaTime;
            if (patrolWaitTimer <= 0) SetNewRandomDestination();
        }
        else
        {
            SetAnimationState(1);
        }

        if (CanSeePlayer())
        {
            currentState = AIState.Chasing;
        }
    }

    private void SetNewRandomDestination()
    {
        Vector2 randomDirection = Random.insideUnitCircle * patrolRadius;
        Vector3 targetPosition = startPosition + new Vector3(randomDirection.x, randomDirection.y, 0);
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, patrolRadius, 1))
        {
            agent.SetDestination(hit.position);
            patrolWaitTimer = Random.Range(patrolWaitTimeRange.x, patrolWaitTimeRange.y);
        }
    }

    private void HandleChasingState()
    {
        agent.isStopped = false;
        SetAnimationState(1);

        if (CanSeePlayer()) timeSinceLostSight = 0f;
        else timeSinceLostSight += Time.deltaTime;

        if (timeSinceLostSight >= persistenceDuration)
        {
            currentState = AIState.Patrolling;
            SetNewRandomDestination();
            return;
        }

        agent.SetDestination(player.transform.position);
        FaceTarget(player.transform.position);

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            currentState = AIState.Attacking;
            agent.isStopped = true;
            SetAnimationState(0);
        }
    }

    private void HandleAttackingState()
    {
        agent.isStopped = true;
        FaceTarget(player.transform.position);
        SetAnimationState(0);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            animControl.TriggerAttack();
        }

        if (Vector2.Distance(transform.position, player.transform.position) > agent.stoppingDistance + 0.5f)
        {
            currentState = AIState.Chasing;
        }
    }

    public void OnTakeDamage()
    {
        if (selfDamageable.IsDead()) return;
        if (currentState == AIState.Patrolling)
        {
            currentState = AIState.Chasing;
            timeSinceLostSight = 0;
        }
        FaceTarget(player.transform.position);
    }

    // << MÉTODO VAZIO ADICIONADO AQUI >>
    public void OnAttackAnimationFinished()
    {
        // Esta função existe apenas para receber o evento da animação e evitar o aviso no console.
    }

    private bool CanSeePlayer() { if (player == null) return false; if (Vector2.Distance(transform.position, player.transform.position) > visionRange) return false; Vector2 directionToPlayer = (player.transform.position - transform.position).normalized; if (Vector2.Angle(transform.right, directionToPlayer) > visionAngle / 2) return false; if (Physics2D.Linecast(transform.position, player.transform.position, visionObstacleMask).collider != null) return false; return true; }
    private void FaceTarget(Vector3 targetPosition) { if (targetPosition.x < transform.position.x) transform.localScale = new Vector3(-1f, 1f, 1f); else if (targetPosition.x > transform.position.x) transform.localScale = new Vector3(1f, 1f, 1f); }
    private void OnDrawGizmosSelected() { if (player != null) { Gizmos.color = Color.gray; Gizmos.DrawWireSphere(startPosition, patrolRadius); Gizmos.color = Color.white; Gizmos.DrawWireSphere(transform.position, visionRange); Vector3 rightLimit = Quaternion.Euler(0, 0, visionAngle / 2) * transform.right; Vector3 leftLimit = Quaternion.Euler(0, 0, -visionAngle / 2) * transform.right; Gizmos.color = Color.yellow; Gizmos.DrawLine(transform.position, transform.position + (transform.right * visionRange)); Gizmos.DrawLine(transform.position, transform.position + rightLimit * visionRange); Gizmos.DrawLine(transform.position, transform.position + leftLimit * visionRange); } }
}