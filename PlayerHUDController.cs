using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDController : MonoBehaviour
{
    private Player player;

    [Header("Componentes da Barra de Vida (HP)")]
    [SerializeField] private Image hpFillImage;

    [Header("Componentes da Barra de Estamina")]
    [SerializeField] private Image staminaFillImage;

    void Start()
    {
        // Procura ativamente na cena por um objeto com a tag "Player".
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.GetComponent<Player>();
        }

        // Se, mesmo depois da busca, não encontrar o jogador, loga um erro claro.
        if (player == null)
        {
            Debug.LogError("PlayerHUDController: Não foi possível encontrar um GameObject com o tag 'Player' ou o objeto não tem o script 'Player.cs'. Desativando o script da HUD.", this);
            this.enabled = false;
        }
    }

    // Usamos LateUpdate para garantir que a UI só seja atualizada DEPOIS que toda a lógica do jogador no Update() já rodou.
    private void LateUpdate()
    {
        // Se a referência do player não for encontrada, o script não continua.
        if (player == null) return;

        // Garante que maxHealth não seja zero para evitar divisão por zero.
        if (player.maxHealth > 0)
        {
            // Calcula a porcentagem de vida (um valor entre 0.0 e 1.0) e atribui ao preenchimento da imagem.
            hpFillImage.fillAmount = player.currentHealth / player.maxHealth;
        }

        if (player.maxStamina > 0)
        {
            staminaFillImage.fillAmount = player.currentStamina / player.maxStamina;
        }
    }
}