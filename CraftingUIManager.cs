using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class CraftingUIManager : MonoBehaviour
{
    public static CraftingUIManager instance;

    [Header("Componentes da UI")]
    [SerializeField] private GameObject craftingPanel;
    [SerializeField] private Transform recipeListContent;
    [SerializeField] private GameObject recipeUIPrefab;

    [Header("Área de Detalhes")]
    [SerializeField] private Image resultItemIcon;
    [SerializeField] private TextMeshProUGUI resultItemName;
    // O campo da descrição foi removido pois não é mais necessário
    // [SerializeField] private TextMeshProUGUI resultItemDescription; 
    [SerializeField] private Transform ingredientListContent;
    [SerializeField] private GameObject ingredientUIPrefab;
    [SerializeField] private Button craftButton;

    private List<CraftingRecipe> allRecipes;
    private CraftingRecipe selectedRecipe;
    private Player playerRef;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        allRecipes = Resources.LoadAll<CraftingRecipe>("Recipes").ToList();

        if (craftingPanel != null)
        {
            craftingPanel.SetActive(false);
        }
    }

    private void Start()
    {
        playerRef = FindObjectOfType<Player>();
    }

    private void Update()
    {
        if (craftingPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void Open(CraftingStationType stationType)
    {
        craftingPanel.SetActive(true);
        if (playerRef != null) playerRef.isPaused = true;

        selectedRecipe = null;
        UpdateDetailsPanel();

        foreach (Transform child in recipeListContent)
        {
            Destroy(child.gameObject);
        }

        var recipesForStation = allRecipes.Where(r => r.requiredStation == stationType).ToList();

        foreach (var recipe in recipesForStation)
        {
            GameObject recipeUIObject = Instantiate(recipeUIPrefab, recipeListContent);
            recipeUIObject.GetComponent<RecipeEntryUI>().Setup(recipe);
        }
    }

    public void Close()
    {
        if (playerRef != null) playerRef.isPaused = false;

        if (craftingPanel != null)
        {
            craftingPanel.SetActive(false);
        }
    }

    public void SelectRecipe(CraftingRecipe recipe)
    {
        selectedRecipe = recipe;
        UpdateDetailsPanel();
    }

    private void UpdateDetailsPanel()
    {
        // Pega o componente de gatilho do tooltip no nosso ícone
        TooltipTriggerUI tooltipTrigger = resultItemIcon.GetComponent<TooltipTriggerUI>();

        if (selectedRecipe != null)
        {
            resultItemIcon.gameObject.SetActive(true);
            resultItemIcon.sprite = selectedRecipe.resultItem.icon;
            resultItemName.text = selectedRecipe.resultItem.itemName;

            // --- LÓGICA ATUALIZADA AQUI ---
            // Informa ao gatilho qual é o ItemData que ele deve mostrar no tooltip
            if (tooltipTrigger != null)
            {
                tooltipTrigger.Setup(selectedRecipe.resultItem);
            }
            // -----------------------------

            foreach (Transform child in ingredientListContent)
            {
                Destroy(child.gameObject);
            }

            foreach (var ingredient in selectedRecipe.ingredients)
            {
                GameObject ingredientUIObject = Instantiate(ingredientUIPrefab, ingredientListContent);
                LayoutElement le = ingredientUIObject.GetComponent<LayoutElement>();
                if (le == null) le = ingredientUIObject.AddComponent<LayoutElement>();
                le.minHeight = 30;

                ingredientUIObject.GetComponent<IngredientEntryUI>().Setup(ingredient);
            }

            // Use o nome correto da sua classe de inventário aqui
            craftButton.interactable = PlayerItens.instance.HasIngredients(selectedRecipe.ingredients);
        }
        else
        {
            resultItemIcon.gameObject.SetActive(false);
            resultItemName.text = "Selecione uma receita";

            // Limpa a referência do gatilho quando nenhuma receita está selecionada
            if (tooltipTrigger != null) tooltipTrigger.Setup(null);

            foreach (Transform child in ingredientListContent) { Destroy(child.gameObject); }
            craftButton.interactable = false;
        }
    }

    public void OnCraftButtonClick()
    {
        if (selectedRecipe != null)
        {
            CraftingManager.instance.Craft(selectedRecipe);
            UpdateDetailsPanel();
        }
    }
}