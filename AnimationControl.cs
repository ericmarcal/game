using UnityEngine;

public class AnimationControl : MonoBehaviour
{
    [Header("Ataque do Inimigo")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float radius;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float attackDamage = 1f;

    private Animator anim;
    private static readonly int TransitionHash = Animator.StringToHash("transition");
    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayAnim(int value)
    {
        if (anim != null)
        {
            anim.SetInteger(TransitionHash, value);
        }
    }

    public void TriggerAttack()
    {
        if (anim != null)
        {
            anim.SetTrigger(AttackTriggerHash);
        }
    }

    public void Attack()
    {
        if (attackPoint == null)
        {
            //Debug.LogError($"AttackPoint não foi atribuído no inimigo {gameObject.name}!", this);
            return;
        }
        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, radius, playerLayer);
        hit?.GetComponent<IDamageable>()?.TakeDamage(attackDamage);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, radius);
        }
    }
}