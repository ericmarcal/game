using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CursedAltar : MonoBehaviour
{
    [Header("Configuração da Transformação")]
    [Tooltip("O prefab do inimigo no qual o jogador se transformará.")]
    [SerializeField] private GameObject enemyToBecomePrefab;
    [Tooltip("A tecla para interagir com o altar.")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    private bool playerInRange = false;
    private Player playerInTrigger = null;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            if (GameManager.instance != null && playerInTrigger != null)
            {
                // Desativa o altar para não ser usado de novo
                this.enabled = false;
                // Chama o GameManager para iniciar a transformação
                GameManager.instance.TransformPlayerIntoEnemy(playerInTrigger, enemyToBecomePrefab);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se é o jogador e pega uma referência do seu script
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            playerInRange = true;
            playerInTrigger = player;
            // Aqui poderíamos mostrar um prompt de interação
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerInTrigger = null;
            // Aqui esconderíamos o prompt de interação
        }
    }
}