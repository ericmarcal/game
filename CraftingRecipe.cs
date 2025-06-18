using UnityEngine;
using System.Collections.Generic;

// Esta pequena classe para definir um ingrediente continua a mesma
[System.Serializable]
public class Ingredient
{
    public ItemData item;
    public int quantity;
}

// NOVO ENUM: Define os tipos de estação de trabalho disponíveis
public enum CraftingStationType
{
    None, // Para crafting direto do inventário (se quisermos no futuro)
    Furnace,
    Campfire,
    Anvil
}


[CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Crafting/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    // --- NOVO CAMPO ADICIONADO AQUI ---
    [Header("Estação de Trabalho")]
    [Tooltip("Qual estação é necessária para criar esta receita? 'None' significa que pode ser criada em qualquer lugar.")]
    public CraftingStationType requiredStation;
    // ------------------------------------

    [Header("Ingredientes Necessários")]
    [Tooltip("A lista de itens e suas respectivas quantidades para criar a receita.")]
    public List<Ingredient> ingredients;

    [Header("Item Resultante")]
    [Tooltip("O item que será criado a partir desta receita.")]
    public ItemData resultItem;
    [Tooltip("A quantidade do item resultante que será criada.")]
    [Range(1, 999)]
    public int resultQuantity = 1;
}