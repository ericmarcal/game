using UnityEngine;
using UnityEngine.Tilemaps; // Necessário para controlar Tilemaps
using System.Collections;

public class RoofFader : MonoBehaviour
{
    [Header("Configurações")]
    [Tooltip("Arraste para cá o GameObject que contém o Tilemap do seu telhado.")]
    [SerializeField] private Tilemap roofTilemap;
    [Tooltip("A velocidade com que o fade acontece.")]
    [SerializeField] private float fadeSpeed = 2f;

    private Coroutine currentFadeCoroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Se já houver uma corrotina de fade rodando, para ela
            if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
            // Inicia a corrotina para fazer o fade de desaparecimento
            currentFadeCoroutine = StartCoroutine(FadeRoof(0f)); // 0f = Transparente
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
            // Inicia a corrotina para fazer o fade de reaparição
            currentFadeCoroutine = StartCoroutine(FadeRoof(1f)); // 1f = Opaco
        }
    }

    private IEnumerator FadeRoof(float targetAlpha)
    {
        // Pega a cor atual do tilemap
        Color currentColor = roofTilemap.color;
        float startAlpha = currentColor.a;

        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * fadeSpeed;
            // Calcula a nova transparência (alpha) usando uma interpolação suave
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime);
            // Aplica a nova cor ao tilemap
            roofTilemap.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null; // Espera o próximo frame
        }

        // Garante que o alpha final seja exatamente o alvo
        roofTilemap.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
    }
}