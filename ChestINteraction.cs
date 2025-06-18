using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ChestInventory))]
public class ChestInteraction : MonoBehaviour
{
    [Header("Configuração de Interação")]
    [Tooltip("A tecla que o jogador pressiona para abrir o baú.")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("Configuração de Animação")]
    [Tooltip("O nome EXATO do estado de animação de ABRIR no Animator.")]
    [SerializeField] private string openAnimationName = "Chest_Open";
    [Tooltip("O nome EXATO do estado de animação de FECHAR no Animator.")]
    [SerializeField] private string closeAnimationName = "Chest_Close";

    // Controle de estado
    private bool playerInRange = false;
    private bool isChestOpen = false;

    // Referências de componentes
    private Player player;
    private Animator animator;
    private ChestInventory chestInventory;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        chestInventory = GetComponent<ChestInventory>();
    }

    private void Start()
    {
        player = FindObjectOfType<Player>();
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            if (!isChestOpen)
            {
                OpenChest();
            }
            else
            {
                CloseChest();
            }
        }

        if (isChestOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseChest();
            }
        }
    }

    private void OpenChest()
    {
        isChestOpen = true;
        // Toca a animação de ABRIR diretamente pelo nome
        animator.Play(openAnimationName);
        ChestUIManager.instance.OpenChestUI(chestInventory);
    }

    private void CloseChest()
    {
        isChestOpen = false;
        // Toca a animação de FECHAR diretamente pelo nome
        animator.Play(closeAnimationName);
        ChestUIManager.instance.CloseChestUI();

        if (InventoryManager.instance != null && InventoryManager.instance.IsInventoryOpen())
        {
            InventoryManager.instance.ToggleInventory(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (isChestOpen)
            {
                CloseChest();
            }
        }
    }
}