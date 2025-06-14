using UnityEngine;
using UnityEngine.UI; // Necessário para Slider

public class PlayerStatsUI : MonoBehaviour
{
    [Header("Referências dos Componentes")]
    [SerializeField] private Player player;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider staminaSlider;

    private void Start()
    {
        // Garante que as barras comecem cheias
        if (player != null)
        {
            healthSlider.maxValue = player.maxHealth;
            healthSlider.value = player.maxHealth;
            staminaSlider.maxValue = player.maxStamina;
            staminaSlider.value = player.maxStamina;
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Atualiza o valor dos sliders a cada frame
        healthSlider.value = player.currentHealth;
        staminaSlider.value = player.currentStamina;
    }
}