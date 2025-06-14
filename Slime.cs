using UnityEngine;
using System.Collections;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyController))]
public class Slime : MonoBehaviour, IDamageable
{
    public enum SlimeSize { Grande, Medio, Pequeno }

    [Header("Atributos do Slime")]
    [SerializeField] private SlimeSize slimeSize = SlimeSize.Grande;
    [SerializeField] private float maxHealth = 10f;
    private float currentHealth;

    [Header("Dano e Efeitos")]
    [SerializeField] private float contactDamage = 1f;
    [SerializeField] private float damageTickRate = 1.0f;

    [Header("Comportamento de Divisão")]
    [SerializeField] private GameObject slimeToSpawnOnDeath;
    [SerializeField] private int numberOfSpawns = 2;
    [SerializeField] private float spawnPopForce = 2.5f;

    [Header("Feedback Visual")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    private EnemyController aiController;
    private AnimationControl animControl;
    private Player player;
    private SpriteRenderer spriteRenderer;
    private Color originalSpriteColor;
    private bool isSlowingPlayer = false;
    private Coroutine damageCoroutine;

    public bool IsDead() => currentHealth <= 0;

    private void Awake()
    {
        aiController = GetComponent<EnemyController>();
        animControl = GetComponent<AnimationControl>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        player = FindObjectOfType<Player>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        if (spriteRenderer != null) originalSpriteColor = spriteRenderer.color;
    }

    public void TakeDamage(float damageAmount)
    {
        if (IsDead()) return;
        currentHealth -= damageAmount;
        if (aiController != null) aiController.OnTakeDamage();
        StartCoroutine(DamageFlashFeedback());
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (animControl != null) animControl.PlayAnim(3);

        if (slimeSize != SlimeSize.Pequeno && slimeToSpawnOnDeath != null && numberOfSpawns > 0)
        {
            for (int i = 0; i < numberOfSpawns; i++)
            {
                Vector3 spawnOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.3f, 0.3f), 0);
                GameObject newSlimeInstance = Instantiate(slimeToSpawnOnDeath, transform.position + spawnOffset, Quaternion.identity);
                Rigidbody2D rbChild = newSlimeInstance.GetComponent<Rigidbody2D>();
                if (rbChild != null)
                {
                    Vector2 popDirection = new Vector2(Random.Range(-1f, 1f), 1f).normalized;
                    rbChild.AddForce(popDirection * spawnPopForce, ForceMode2D.Impulse);
                }
            }
        }

        if (isSlowingPlayer && player != null) player.RemoveSlow();
        if (damageCoroutine != null) StopCoroutine(damageCoroutine);
        if (GetComponent<NavMeshAgent>() != null) GetComponent<NavMeshAgent>().enabled = false;
        if (aiController != null) aiController.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        Destroy(gameObject, 2f);
    }

    // A lógica de Trigger para dano e lentidão continua sendo um comportamento único do Slime
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsDead()) return;
        if (other.CompareTag("Player"))
        {
            Player playerComponent = other.GetComponent<Player>();
            if (playerComponent != null)
            {
                playerComponent.ApplySlow();
                isSlowingPlayer = true;
                if (damageCoroutine == null) damageCoroutine = StartCoroutine(DealDamageOverTime(playerComponent));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsDead()) return;
        if (other.CompareTag("Player"))
        {
            Player playerComponent = other.GetComponent<Player>();
            if (playerComponent != null)
            {
                playerComponent.RemoveSlow();
                isSlowingPlayer = false;
                if (damageCoroutine != null)
                {
                    StopCoroutine(damageCoroutine);
                    damageCoroutine = null;
                }
            }
        }
    }

    private IEnumerator DealDamageOverTime(Player playerToDamage)
    {
        while (true)
        {
            playerToDamage.TakeDamage(contactDamage);
            yield return new WaitForSeconds(damageTickRate);
        }
    }

    private IEnumerator DamageFlashFeedback()
    {
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalSpriteColor;
    }
}