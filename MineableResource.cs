using UnityEngine;
using System.Collections;

public class MineableResource : MonoBehaviour
{
    [Header("Configuração do Recurso")]
    public int maxHealth = 3;
    private int currentHealth;

    [Tooltip("Os sprites para cada estágio de 'dano' do minério. O primeiro (índice 0) é o minério intacto.")]
    public Sprite[] damageStagesSprites;

    [Header("Configuração do Drop")] // << HEADER ATUALIZADO
    [Tooltip("O ItemData do recurso que será dropado.")]
    public ItemData itemToDrop;
    [Tooltip("A quantidade do item a ser dropada.")]
    public int quantityToDrop = 1;
    [Tooltip("A força com que o item 'pula' ao ser dropado.")]
    [SerializeField] private float dropPopForce = 1.5f; // << NOVO

    [Header("Respawn (Opcional)")]
    public bool canRespawn = true;
    public float respawnTime = 10f;

    [Header("Feedback Visual de Hit")]
    public Color hitFlashColor = Color.white;
    public float hitFlashDuration = 0.1f;
    public int numberOfFlashes = 1;
    public float shakeIntensity = 0.05f;
    public float shakeDuration = 0.15f;

    private SpriteRenderer spriteRenderer;
    private Collider2D DesteCollider2D;
    private bool isDestroyed = false;
    private UnityEngine.AI.NavMeshObstacle navMeshObstacle;

    private Color originalSpriteColor;
    private Coroutine hitFeedbackCoroutine;
    private Vector3 originalPosition;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        DesteCollider2D = GetComponent<Collider2D>();

        if (TryGetComponent<UnityEngine.AI.NavMeshObstacle>(out var obstacle))
        {
            navMeshObstacle = obstacle;
        }

        if (spriteRenderer != null)
        {
            originalSpriteColor = spriteRenderer.color;
        }
        originalPosition = transform.position;
        InitializeResource();
    }

    void InitializeResource()
    {
        currentHealth = maxHealth;
        isDestroyed = false;
        if (DesteCollider2D != null) DesteCollider2D.enabled = true;
        if (navMeshObstacle != null) navMeshObstacle.enabled = true;
        else if (GetComponent<UnityEngine.AI.NavMeshObstacle>() != null)
        {
            navMeshObstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();
            if (navMeshObstacle != null) navMeshObstacle.enabled = true;
        }

        UpdateSprite();
        transform.position = originalPosition;
        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }

    public void OnHit(int damageAmount = 1)
    {
        if (isDestroyed || currentHealth <= 0) return;

        currentHealth -= damageAmount;

        if (hitFeedbackCoroutine != null)
        {
            StopCoroutine(hitFeedbackCoroutine);

            if (spriteRenderer != null) spriteRenderer.color = originalSpriteColor;
            transform.position = originalPosition;
        }
        hitFeedbackCoroutine = StartCoroutine(HitFeedbackCoroutine());

        UpdateSprite();

        if (currentHealth <= 0)
        {
            DestroyResource();
        }
    }

    IEnumerator HitFeedbackCoroutine()
    {
        originalPosition = transform.position;

        if (spriteRenderer != null)
        {
            for (int i = 0; i < numberOfFlashes; i++)
            {
                spriteRenderer.color = hitFlashColor;
                yield return new WaitForSeconds(hitFlashDuration / (numberOfFlashes * 2));
                spriteRenderer.color = originalSpriteColor;
                yield return new WaitForSeconds(hitFlashDuration / (numberOfFlashes * 2));
            }
            spriteRenderer.color = originalSpriteColor;
        }

        float elapsed = 0.0f;
        while (elapsed < shakeDuration)
        {
            float x = originalPosition.x + Random.Range(-1f, 1f) * shakeIntensity;
            float y = originalPosition.y + Random.Range(-1f, 1f) * shakeIntensity;
            transform.position = new Vector3(x, y, originalPosition.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPosition;
        hitFeedbackCoroutine = null;
    }

    void UpdateSprite()
    {
        if (spriteRenderer == null || damageStagesSprites == null || damageStagesSprites.Length == 0) return;

        int spriteIndex = Mathf.Max(0, maxHealth - currentHealth);

        if (currentHealth <= 0)
        {
            spriteIndex = damageStagesSprites.Length - 1;
        }

        spriteIndex = Mathf.Clamp(spriteIndex, 0, damageStagesSprites.Length - 1);

        if (damageStagesSprites[spriteIndex] != null)
        {
            spriteRenderer.sprite = damageStagesSprites[spriteIndex];
        }
    }

    void DestroyResource()
    {
        isDestroyed = true;

        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (DesteCollider2D != null) DesteCollider2D.enabled = false;
        if (navMeshObstacle != null) navMeshObstacle.enabled = false;

        DropItems();

        if (canRespawn)
        {
            StartCoroutine(RespawnTimer());
        }
        else
        {
            Destroy(gameObject, 0.1f);
        }
    }

    void DropItems()
    {
        if (itemToDrop == null || itemToDrop.itemPrefab == null || quantityToDrop <= 0) return;

        if (quantityToDrop > 0)
        {
            Vector3 dropPosition = transform.position + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
            GameObject droppedItemGO = Instantiate(itemToDrop.itemPrefab, dropPosition, Quaternion.identity);
            WorldItem worldItemScript = droppedItemGO.GetComponent<WorldItem>();
            if (worldItemScript != null)
            {
                worldItemScript.itemData = itemToDrop;
                worldItemScript.quantity = quantityToDrop;

                // << LÓGICA DE DROP ADICIONADA PARA CONSISTÊNCIA >>
                Vector2 popDirection = new Vector2(Random.Range(-0.5f, 0.5f), 1f);
                worldItemScript.SetupSpawnedItemParameters(dropPosition, popDirection, dropPopForce);
            }
            else
            {
                //Debug.LogError($"O prefab '{itemToDrop.itemPrefab.name}' para '{itemToDrop.itemName}' não tem o script WorldItem!", droppedItemGO);
            }
        }
    }

    IEnumerator RespawnTimer()
    {
        yield return new WaitForSeconds(respawnTime);
        InitializeResource();
    }
}