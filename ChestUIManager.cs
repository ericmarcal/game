using UnityEngine;
using UnityEngine.UI; // Necessário para GridLayoutGroup
using System.Collections.Generic;

public class ChestUIManager : MonoBehaviour
{
    public static ChestUIManager instance;

    [Header("Componentes da UI")]
    [SerializeField] private GameObject chestPanel;
    [SerializeField] private Transform gridParent;
    [SerializeField] private GameObject slotPrefab;

    // << NOVA REFERÊNCIA NECESSÁRIA >>
    [Tooltip("Arraste aqui o objeto que contém o componente Grid Layout Group.")]
    [SerializeField] private GridLayoutGroup gridLayoutGroup;

    public ChestInventory currentChestInventory { get; private set; }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        if (chestPanel != null) chestPanel.SetActive(false);
    }

    public void OpenChestUI(ChestInventory chestToOpen)
    {
        currentChestInventory = chestToOpen;
        if (chestPanel != null) chestPanel.SetActive(true);
        if (InventoryManager.instance != null) InventoryManager.instance.ToggleInventory(true);
        UpdateDisplay();
    }

    public void CloseChestUI()
    {
        currentChestInventory = null;
        if (chestPanel != null) chestPanel.SetActive(false);
    }

    public void UpdateDisplay()
    {
        if (currentChestInventory == null || gridParent == null || slotPrefab == null) return;

        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < currentChestInventory.chestSlots.Count; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, gridParent);
            var slotUI = slotGO.GetComponent<InventorySlotUI>();
            if (slotUI != null)
            {
                slotUI.Link(i, ContainerType.Chest);
            }
        }

        // Após criar os slots, redimensiona o painel
        ResizePanelToFitContent();
    }

    // << NOVO MÉTODO PARA REDIMENSIONAR O PAINEL >>
    private void ResizePanelToFitContent()
    {
        if (gridLayoutGroup == null || currentChestInventory.chestSlots.Count == 0) return;

        // Pega as configurações do Grid
        float padTop = gridLayoutGroup.padding.top;
        float padLeft = gridLayoutGroup.padding.left;
        float padBottom = gridLayoutGroup.padding.bottom;
        float padRight = gridLayoutGroup.padding.right;
        float spacingX = gridLayoutGroup.spacing.x;
        float spacingY = gridLayoutGroup.spacing.y;
        float cellWidth = gridLayoutGroup.cellSize.x;
        float cellHeight = gridLayoutGroup.cellSize.y;
        int columnCount = gridLayoutGroup.constraintCount;

        // Calcula o número de linhas necessárias
        int rowCount = Mathf.CeilToInt((float)currentChestInventory.chestSlots.Count / columnCount);

        // Calcula a altura e largura final
        float totalWidth = padLeft + padRight + (columnCount * cellWidth) + ((columnCount - 1) * spacingX);
        float totalHeight = padTop + padBottom + (rowCount * cellHeight) + ((rowCount - 1) * spacingY);

        // Aplica o tamanho ao RectTransform do painel principal
        RectTransform panelRect = chestPanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.sizeDelta = new Vector2(totalWidth, totalHeight);
        }
    }
}