using UnityEngine;

public class NPCFollower : MonoBehaviour
{
    [Tooltip("Velocidade constante com que o NPC seguirá o jogador.")]
    public float followSpeed = 3f;
    [Tooltip("Distância máxima para o jogador poder chamar/dispensar o NPC.")]
    public float interactionRange = 10f;
    [Tooltip("Distância que o NPC mantém do jogador ao seguir.")]
    public float stopDistance = 2.0f;
    public string playerTag = "Player";

    private Transform playerTransform;
    private bool isFollowing = false;
    private bool isMovingToStopPoint = false;
    private Vector3 stopTargetPosition;

    private Rigidbody2D rb;
    private NPC npcPatrolScript;

    private bool _shouldMoveInFixedUpdate = false;
    private Vector2 _targetPositionForFixedUpdate;

    public static NPCFollower currentActiveFollower;
    public static float lastCommandTime = -1f;
    public static float commandCooldown = 0.2f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        npcPatrolScript = GetComponent<NPC>();
        //if (rb == null) Debug.LogWarning($"NPCFollower em {gameObject.name} não tem Rigidbody2D!", this);

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;
        float distanceToPlayer = Vector2.Distance(GetCurrentPosition(), playerTransform.position);
        _shouldMoveInFixedUpdate = false;

        if (Input.GetKeyDown(KeyCode.Q) && Time.time - lastCommandTime >= commandCooldown)
        {
            lastCommandTime = Time.time;
            if (isFollowing && currentActiveFollower == this)
            {
                isFollowing = false;
                isMovingToStopPoint = true;
                stopTargetPosition = playerTransform.position;
            }
            else if (!isFollowing && distanceToPlayer <= interactionRange)
            {
                if (currentActiveFollower == null || (currentActiveFollower == this && !isMovingToStopPoint))
                {
                    if (currentActiveFollower != null && currentActiveFollower != this)
                        currentActiveFollower.ForceStopAndRelease(true);

                    isFollowing = true;
                    isMovingToStopPoint = false;
                    currentActiveFollower = this;
                    if (npcPatrolScript != null) npcPatrolScript.enabled = false;

                    FaceTarget(playerTransform.position);
                }
            }
        }

        if (isFollowing)
        {
            if (distanceToPlayer > stopDistance)
            {
                _targetPositionForFixedUpdate = playerTransform.position;
                _shouldMoveInFixedUpdate = true;
            }
            else
            {
                _shouldMoveInFixedUpdate = false;
                FaceTarget(playerTransform.position);
            }
        }
        else if (isMovingToStopPoint)
        {
            if (Vector2.Distance(GetCurrentPosition(), stopTargetPosition) > 0.1f)
            {
                _targetPositionForFixedUpdate = stopTargetPosition;
                _shouldMoveInFixedUpdate = true;
            }
            else
            {
                isMovingToStopPoint = false;
                _shouldMoveInFixedUpdate = false;
                if (currentActiveFollower != this && npcPatrolScript != null)
                {
                    npcPatrolScript.enabled = true;
                }
            }
        }
        else
        {
            if (currentActiveFollower != this && npcPatrolScript != null && !npcPatrolScript.enabled)
            {
                npcPatrolScript.enabled = true;
            }
        }
    }

    public void ForceStopAndRelease(bool enablePatrol)
    {
        isFollowing = false;
        isMovingToStopPoint = false;
        _shouldMoveInFixedUpdate = false;
        if (currentActiveFollower == this)
        {
            currentActiveFollower = null;
        }
        if (enablePatrol && npcPatrolScript != null)
        {
            npcPatrolScript.enabled = true;
        }
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        if (_shouldMoveInFixedUpdate)
        {
            Vector2 currentPos = rb.position;
            Vector2 directionToTarget = (_targetPositionForFixedUpdate - currentPos).normalized;

            if (directionToTarget == Vector2.zero)
            {
                _shouldMoveInFixedUpdate = false;
                if (isMovingToStopPoint)
                {
                    isMovingToStopPoint = false;
                    if (currentActiveFollower != this && npcPatrolScript != null) npcPatrolScript.enabled = true;
                }
                return;
            }

            Vector2 newPosition = currentPos + directionToTarget * followSpeed * Time.fixedDeltaTime;

            if (!isFollowing && isMovingToStopPoint)
            {
                if (Vector2.Distance(currentPos, _targetPositionForFixedUpdate) < (directionToTarget * followSpeed * Time.fixedDeltaTime).magnitude)
                {
                    newPosition = _targetPositionForFixedUpdate;
                    _shouldMoveInFixedUpdate = false;
                    isMovingToStopPoint = false;
                    if (currentActiveFollower != this && npcPatrolScript != null) npcPatrolScript.enabled = true;
                }
            }
            rb.MovePosition(newPosition);
        }

        if (isFollowing && playerTransform != null)
        {
            FaceTarget(playerTransform.position);
        }
        else if (_shouldMoveInFixedUpdate)
        {
            FaceTarget(_targetPositionForFixedUpdate);
        }
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        if (targetPosition.x < transform.position.x)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (targetPosition.x > transform.position.x)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }

    Vector2 GetCurrentPosition() { return rb != null ? rb.position : (Vector2)transform.position; }

    public void ReleaseAsFollower()
    {
        isFollowing = false;
        isMovingToStopPoint = false;
        if (npcPatrolScript != null)
        {
            npcPatrolScript.enabled = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        if (isFollowing && playerTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, stopDistance);
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
        else if (isMovingToStopPoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, stopTargetPosition);
        }
    }
}