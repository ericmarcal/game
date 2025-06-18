using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlacementGhost : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [Header("Cores de Feedback")]
    [SerializeField] private Color validPlacementColor = new Color(0f, 1f, 0f, 0.5f); // Verde transparente
    [SerializeField] private Color invalidPlacementColor = new Color(1f, 0f, 0f, 0.5f); // Vermelho transparente

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Método público para definir a cor baseada na validade
    public void SetValidity(bool isValid)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isValid ? validPlacementColor : invalidPlacementColor;
        }
    }
}