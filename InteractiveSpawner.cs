using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class InteractiveSpawner : MonoBehaviour
{
    [Header("Configuração do Inimigo")]
    [Tooltip("O Prefab do inimigo que será instanciado.")]
    [SerializeField] private GameObject enemyPrefab;
    [Tooltip("O ponto exato onde o inimigo irá aparecer. Crie um GameObject filho vazio para isso.")]
    [SerializeField] private Transform spawnPoint;

    [Header("Configuração da Interação")]
    [Tooltip("A tecla que o jogador deve pressionar para ativar o spawner.")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [Tooltip("Opcional: Um objeto (como um ícone 'E') que aparece quando o jogador pode interagir.")]
    [SerializeField] private GameObject interactionPrompt;
    [Tooltip("O tempo em segundos que o jogador deve esperar antes de poder usar o spawner novamente.")]
    [SerializeField] private float spawnCooldown = 5f;

    [Header("Feedback Visual")]
    [Tooltip("A cor que o spawner piscará ao ser ativado.")]
    [SerializeField] private Color flashColor = Color.white;
    [Tooltip("A duração em segundos do efeito de flash.")]
    [SerializeField] private float flashDuration = 0.2f;

    // Controle de estado
    private bool playerInRange = false;
    private float lastSpawnTime = -999f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        bool canSpawn = Time.time >= lastSpawnTime + spawnCooldown;
        if (playerInRange && canSpawn && Input.GetKeyDown(interactionKey))
        {
            SpawnCreature();
        }
    }

    private void SpawnCreature()
    {
        if (enemyPrefab == null)
        {
            //Debug.LogError("Nenhum prefab de inimigo foi atribuído no InteractiveSpawner!", this);
            return;
        }

        Vector3 finalSpawnPosition = (spawnPoint != null) ? spawnPoint.position : transform.position;
        Instantiate(enemyPrefab, finalSpawnPosition, Quaternion.identity);
        lastSpawnTime = Time.time;
        StartCoroutine(FlashEffect());
    }

    private IEnumerator FlashEffect()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
}