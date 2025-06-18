using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("Referências dos Componentes")]
    [SerializeField] private Player player;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider staminaSlider;

    private bool isInitialized = false;

    private void Update()
    {
        if (player == null)
        {
            if (Time.frameCount < 2) Debug.LogError("PlayerStatsUI: A referência do 'Player' está NULA!");
            return;
        }

        if (!isInitialized && player.maxHealth > 0)
        {
            healthSlider.maxValue = player.maxHealth;
            staminaSlider.maxValue = player.maxStamina;
            isInitialized = true;
            Debug.Log($"<color=cyan>PlayerStatsUI (Inicialização):</color> MaxValue da barra de HP definido para {healthSlider.maxValue}");
        }

        healthSlider.value = player.currentHealth;
        staminaSlider.value = player.currentStamina;

        // Log que só aparece nos primeiros 10 frames para não poluir o console
        if (Time.frameCount < 10)
        {
            Debug.Log($"<color=yellow>PlayerStatsUI (Frame {Time.frameCount}):</color> Vida do Jogador = {player.currentHealth}. Valor da Barra = {healthSlider.value}.");
        }
    }
}