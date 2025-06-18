using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeEntryUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI recipeName;
    private Button button;
    private CraftingRecipe recipe;

    public void Setup(CraftingRecipe recipeToSetup)
    {
        this.recipe = recipeToSetup;
        icon.sprite = recipe.resultItem.icon;
        recipeName.text = recipe.resultItem.itemName;

        button = GetComponent<Button>();
        button.onClick.AddListener(() => CraftingUIManager.instance.SelectRecipe(recipe));
    }
}