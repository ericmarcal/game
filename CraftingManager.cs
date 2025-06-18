using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Craft(CraftingRecipe recipe)
    {
        if (recipe == null)
        {
            Debug.LogWarning("Tentativa de criar um item com receita nula.");
            return;
        }

        // Verifica se o jogador tem os ingredientes necessários
        if (PlayerItens.instance.HasIngredients(recipe.ingredients))
        {
            // Remove os ingredientes do inventário
            PlayerItens.instance.RemoveIngredients(recipe.ingredients);

            // Adiciona o item resultante ao inventário
            PlayerItens.instance.AddItem(recipe.resultItem, recipe.resultQuantity);

            Debug.Log($"Item '{recipe.resultItem.itemName}' x{recipe.resultQuantity} criado com sucesso!");
        }
        else
        {
            Debug.Log("Faltam ingredientes para criar este item.");
            // No futuro, podemos tocar um som de "erro" aqui.
        }
    }
}