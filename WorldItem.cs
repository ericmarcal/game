using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class WorldItem : MonoBehaviour
{
    [Header("Configuração do Item")]
    public ItemData itemData;
    public int quantity;

    [Header("Coleta e Comportamento")]
    public float pickupDelay = 1.0f;
    public bool isMagnetic = true;

    [Header("Efeito de Pulo (Parábola Falsa)")]
    [SerializeField] private float arcDuration = 0.4f;
    [SerializeField] private float arcScaleMultiplier = 1.5f;

    [Header("Distância e Velocidade de Coleta")]
    [SerializeField] private float collectDistance = 0.5f;
    [SerializeField] private float magnetDistance = 2.0f;
    [SerializeField] private float magnetSpeed = 2.5f;

    private bool canBeActivated = false;
    private bool isCollected = false;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Coroutine arcCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        if (GameSettings.instance != null)
        {
            if (rb != null)
            {
                rb.drag = GameSettings.instance.itemLinearDrag;
            }
            int targetLayer = LayerMask.NameToLayer(GameSettings.instance.droppedItemLayerName);
            if (gameObject.layer != targetLayer && targetLayer != -1)
            {
                gameObject.layer = targetLayer;
            }
        }
    }

    // O método LateUpdate() que causava o problema foi REMOVIDO.

    public void SetupSpawnedItemParameters(Vector3 initialPosition, Vector2 forceDirection, float popForce)
    {
        transform.position = initialPosition;
        if (spriteRenderer != null && this.itemData != null && this.itemData.icon != null)
            spriteRenderer.sprite = this.itemData.icon;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.AddForce(forceDirection.normalized * popForce, ForceMode2D.Impulse);
        }

        if (arcCoroutine != null) StopCoroutine(arcCoroutine);
        arcCoroutine = StartCoroutine(ArcEffectCoroutine());

        Invoke(nameof(ActivatePickup), pickupDelay);
    }

    public void AnimatePush(Vector3 startPosition, Vector3 targetPosition, float duration)
    {
        StartCoroutine(AnimatePushCoroutine(startPosition, targetPosition, duration));
    }

    private IEnumerator AnimatePushCoroutine(Vector3 startPosition, Vector3 targetPosition, float duration)
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
        transform.position = targetPosition;
        rb.bodyType = RigidbodyType2D.Dynamic;
        GetComponent<Collider2D>().enabled = true;
        Invoke(nameof(ActivatePickup), pickupDelay);
    }

    private IEnumerator ArcEffectCoroutine() { float elapsedTime = 0f; Vector3 originalScale = transform.localScale; Vector3 peakScale = originalScale * arcScaleMultiplier; while (elapsedTime < arcDuration) { elapsedTime += Time.deltaTime; float progress = elapsedTime / arcDuration; float scaleSin = Mathf.Sin(progress * Mathf.PI); transform.localScale = Vector3.Lerp(originalScale, peakScale, scaleSin); yield return null; } transform.localScale = originalScale; }
    private void ActivatePickup() { canBeActivated = true; }
    void Update() { if (!canBeActivated || isCollected || playerTransform == null || itemData == null || !itemData.isCollectible) return; float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position); if (distanceToPlayer <= collectDistance) { TryCollect(); return; } if (isMagnetic && distanceToPlayer <= magnetDistance) { if (PlayerItens.instance != null && PlayerItens.instance.CanAddItem(itemData, quantity)) { transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, magnetSpeed * Time.deltaTime); } } }
    private void OnTriggerEnter2D(Collider2D other) { if (!canBeActivated || isCollected) return; if (other.CompareTag("Player")) TryCollect(); }
    private void TryCollect() { if (isCollected || itemData == null || !itemData.isCollectible) return; if (PlayerItens.instance != null && PlayerItens.instance.CanAddItem(itemData, quantity)) { int remaining = PlayerItens.instance.AddItem(itemData, quantity); if (remaining == 0) { isCollected = true; Destroy(gameObject); } else { quantity = remaining; } } }
}