using UnityEngine;
using System.Collections;

public class WorldItem : MonoBehaviour
{
    [Header("Configuração do Item")]
    public ItemData itemData;
    public int quantity;

    [Header("Coleta e Comportamento")]
    [Tooltip("Tempo em segundos antes do item poder ser coletado após o spawn.")]
    public float pickupDelay = 1.0f; // Aumentado para 1.0f
    public bool isMagnetic = true;

    [Header("Efeito de Pulo (Parábola Falsa)")]
    [Tooltip("Duração do efeito de pulo em segundos.")]
    [SerializeField] private float arcDuration = 0.4f;
    [Tooltip("O quão 'alto' o item pula (multiplicador de escala no pico do arco).")]
    [SerializeField] private float arcScaleMultiplier = 1.5f;

    [Header("Distância e Velocidade de Coleta")]
    [SerializeField] private float collectDistance = 0.5f;
    [SerializeField] private float magnetDistance = 2.0f;
    [SerializeField] private float magnetSpeed = 2.5f; // Reduzido de 4f para 2.5f

    private bool canBeActivated = false;
    private bool isCollected = false;
    private Transform playerTransform;
    private Rigidbody2D rb;

    private Coroutine arcCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private void Start()
    {
        if (rb != null && rb.velocity == Vector2.zero)
        {
            Invoke(nameof(ActivatePickup), pickupDelay);
        }
    }

    // << MÉTODO ATUALIZADO >>: Agora aceita um 'popForce' como parâmetro.
    public void SetupSpawnedItemParameters(Vector3 initialPosition, Vector2 forceDirection, float popForce)
    {
        transform.position = initialPosition;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && this.itemData != null && this.itemData.icon != null)
            sr.sprite = this.itemData.icon;

        if (rb != null)
        {
            // O valor fixo '3.5f' foi substituído pelo parâmetro 'popForce'.
            rb.AddForce(forceDirection.normalized * popForce, ForceMode2D.Impulse);
        }

        if (arcCoroutine != null) StopCoroutine(arcCoroutine);
        arcCoroutine = StartCoroutine(ArcEffectCoroutine());

        Invoke(nameof(ActivatePickup), pickupDelay);
    }

    private IEnumerator ArcEffectCoroutine()
    {
        float elapsedTime = 0f;
        Vector3 originalScale = transform.localScale;
        Vector3 peakScale = originalScale * arcScaleMultiplier;

        while (elapsedTime < arcDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / arcDuration;

            float scaleSin = Mathf.Sin(progress * Mathf.PI);
            transform.localScale = Vector3.Lerp(originalScale, peakScale, scaleSin);

            yield return null;
        }

        transform.localScale = originalScale;
    }


    private void ActivatePickup()
    {
        canBeActivated = true;
    }

    void Update()
    {
        if (!canBeActivated || isCollected || playerTransform == null || itemData == null || !itemData.isCollectible) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= collectDistance)
        {
            TryCollect();
            return;
        }

        if (isMagnetic && distanceToPlayer <= magnetDistance)
        {
            if (PlayerItens.instance != null && PlayerItens.instance.CanAddItem(itemData, quantity))
            {
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, magnetSpeed * Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canBeActivated || isCollected) return;
        if (other.CompareTag("Player")) TryCollect();
    }

    private void TryCollect()
    {
        if (isCollected || itemData == null || !itemData.isCollectible) return;

        if (PlayerItens.instance != null && PlayerItens.instance.CanAddItem(itemData, quantity))
        {
            int remaining = PlayerItens.instance.AddItem(itemData, quantity);
            if (remaining == 0)
            {
                isCollected = true;
                Destroy(gameObject);
            }
            else
            {
                quantity = remaining;
            }
        }
    }
}