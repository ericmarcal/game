using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class IngredientEntryUI : MonoBehaviour
{
    [Header("Referências da UI")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private GameObject checkmarkObject;

    [Header("Configuração de Feedback")]
    [Tooltip("Cor do ícone quando o jogador NÃO tem os itens necessários.")]
    [SerializeField] private Color insufficientIconColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
    [Tooltip("Cor do texto da quantidade quando o jogador NÃO tem os itens necessários.")]
    [SerializeField] private Color insufficientTextColor = Color.red;
    [Tooltip("Cor padrão do texto da quantidade.")]
    [SerializeField] private Color defaultTextColor = Color.white;


    public void Setup(Ingredient ingredient)
    {
        if (ingredient == null || ingredient.item == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // Configura o ícone
        itemIcon.sprite = ingredient.item.icon;

        // Pega a quantidade que o jogador possui usando o método que criamos
        int countInInventory = PlayerItens.instance.GetItemCount(ingredient.item);

        // Pega a quantidade necessária da receita
        int requiredQuantity = ingredient.quantity;

        // --- MUDANÇA PRINCIPAL AQUI ---
        // Define o texto no formato "Atual / Necessária"
        quantityText.text = $"{countInInventory}/{requiredQuantity}";

        // Verifica se tem o suficiente
        bool hasEnough = countInInventory >= requiredQuantity;

        // Ativa ou desativa o checkmark
        if (checkmarkObject != null)
        {
            checkmarkObject.SetActive(hasEnough);
        }

        // Fornece feedback visual no ícone e no texto da quantidade
        itemIcon.color = hasEnough ? Color.white : insufficientIconColor;
        quantityText.color = hasEnough ? defaultTextColor : insufficientTextColor;
    }
}