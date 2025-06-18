using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CraftingStation : MonoBehaviour
{
    [Header("Configuração da Estação")]
    [Tooltip("Defina o tipo desta estação. Deve corresponder ao tipo nas receitas.")]
    [SerializeField] private CraftingStationType stationType;

    [Header("Interação")]
    [Tooltip("A tecla que o jogador pressiona para abrir a interface de crafting.")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [Tooltip("Opcional: Um objeto (como um ícone 'E') para mostrar que a interação é possível.")]
    [SerializeField] private GameObject interactionPrompt;

    private bool playerInRange = false;

    private void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            // Linha antiga: Debug.Log($"Abrindo a interface de crafting para a estação: {stationType}");
            // NOVA LINHA:
            CraftingUIManager.instance.Open(this.stationType);
        }
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