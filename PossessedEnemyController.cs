using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AnimationControl))]
public class PossessedEnemyController : MonoBehaviour
{
    [Header("Atributos")]
    [SerializeField] private float movementSpeed = 3.5f;

    private Rigidbody2D rb;
    private AnimationControl animControl;
    private Vector2 direction;
    private int currentAnimState = -1;

    private bool isAttacking = false;

    // V-- A FUNÇÃO ESTÁ AQUI --V
    // Este é o método que deve aparecer na sua lista de Animation Events.
    // Ele precisa ser público para que a Unity o encontre.
    public void OnAttackFinished()
    {
        isAttacking = false;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animControl = GetComponent<AnimationControl>();

        EnemyController originalAI = GetComponent<EnemyController>();
        if (originalAI != null) originalAI.enabled = false;

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        rb.isKinematic = false;
        rb.gravityScale = 0f;
        rb.drag = 5f;
    }

    void Update()
    {
        if (!isAttacking)
        {
            direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }
        else
        {
            direction = Vector2.zero;
        }

        int newAnimState = direction.sqrMagnitude > 0.01f ? 1 : 0;
        if (newAnimState != currentAnimState && !isAttacking)
        {
            currentAnimState = newAnimState;
            animControl.PlayAnim(currentAnimState);
        }

        if (direction.x < 0) transform.localScale = new Vector3(-1f, 1f, 1f);
        else if (direction.x > 0) transform.localScale = new Vector3(1f, 1f, 1f);

        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            isAttacking = true;
            // Usaremos o sistema de Trigger que já funciona no inimigo
            animControl.TriggerAttack();
        }
    }

    void FixedUpdate()
    {
        rb.velocity = direction.normalized * movementSpeed;
    }
}