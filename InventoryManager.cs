using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    [Header("Componentes da UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private CanvasGroup inventoryCanvasGroup;
    [SerializeField] private Transform gridParent;
    [SerializeField] private GameObject slotPrefab;

    [Tooltip("Arraste aqui o objeto que contém o componente Grid Layout Group.")]
    [SerializeField] private GridLayoutGroup gridLayoutGroup;

    [Header("Drag and Drop Visuals")]
    [SerializeField] private Image draggedItemIcon;
    [SerializeField] private TextMeshProUGUI draggedItemQuantityText;

    private bool isDragging = false;
    private InventorySlotUI sourceSlotUI;
    private Transform originalIconParent;
    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

    public bool IsDragging() => isDragging;
    public bool IsInventoryOpen() => inventoryCanvasGroup.alpha > 0;
    public InventorySlotUI GetSourceSlotUI() => sourceSlotUI;

    private void Awake()
    {
        if (instance == null) instance = this; else Destroy(gameObject);
        if (inventoryCanvasGroup == null) inventoryCanvasGroup = GetComponent<CanvasGroup>();
        if (draggedItemIcon != null)
        {
            originalIconParent = draggedItemIcon.transform.parent;
            draggedItemIcon.raycastTarget = false;
            draggedItemIcon.gameObject.SetActive(false);
        }
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (inventoryCanvasGroup != null) { inventoryCanvasGroup.alpha = 0f; inventoryCanvasGroup.interactable = false; inventoryCanvasGroup.blocksRaycasts = false; }
    }

    private void Start()
    {
        InitializeSlots();
    }

    private void Update() { if (isDragging) UpdateDragPosition(Input.mousePosition); }

    public void ToggleInventory(bool open)
    {
        // Se o inventário está sendo fechado, esconde o tooltip.
        if (!open)
        {
            // --- CORREÇÃO AQUI ---
            // Chamando o método 'Hide()' do 'TooltipSystem'
            if (TooltipSystem.instance != null)
            {
                TooltipSystem.instance.Hide();
            }
        }

        if (!open && isDragging) CancelDrag();
        Player playerRef = FindObjectOfType<Player>();
        if (playerRef != null)
        {
            bool isChestOpen = (ChestUIManager.instance != null && ChestUIManager.instance.currentChestInventory != null);
            playerRef.isPaused = open || isChestOpen;
        }
        if (inventoryPanel != null) inventoryPanel.SetActive(open);
        if (inventoryCanvasGroup != null)
        {
            inventoryCanvasGroup.alpha = open ? 1f : 0f;
            inventoryCanvasGroup.interactable = open;
            inventoryCanvasGroup.blocksRaycasts = open;
        }
        if (!open && StackSplitterUI.instance != null) StackSplitterUI.instance.OnCancelClick();
        if (open) UpdateDisplay();
    }

    private void InitializeSlots()
    {
        for (int i = 0; i < PlayerItens.instance.inventorySize; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, gridParent);
            var slotUI = slotGO.GetComponent<InventorySlotUI>();
            slotUI.Link(i, ContainerType.Inventory);
            uiSlots.Add(slotUI);
        }
    }

    public void UpdateDisplay()
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (i < PlayerItens.instance.inventorySlots.Count)
            {
                uiSlots[i].UpdateVisuals();
            }
        }
        ResizePanelToFitContent();
    }

    private void ResizePanelToFitContent()
    {
        if (gridLayoutGroup == null || uiSlots.Count == 0) return;

        float padTop = gridLayoutGroup.padding.top;
        float padLeft = gridLayoutGroup.padding.left;
        float padBottom = gridLayoutGroup.padding.bottom;
        float padRight = gridLayoutGroup.padding.right;
        float spacingX = gridLayoutGroup.spacing.x;
        float spacingY = gridLayoutGroup.spacing.y;
        float cellWidth = gridLayoutGroup.cellSize.x;
        float cellHeight = gridLayoutGroup.cellSize.y;
        int columnCount = gridLayoutGroup.constraintCount;

        int rowCount = Mathf.CeilToInt((float)PlayerItens.instance.inventorySize / columnCount);

        float totalWidth = padLeft + padRight + (columnCount * cellWidth) + ((columnCount - 1) * spacingX);
        float totalHeight = padTop + padBottom + (rowCount * cellHeight) + ((rowCount - 1) * spacingY);

        RectTransform panelRect = inventoryPanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.sizeDelta = new Vector2(totalWidth, totalHeight);
        }
    }

    #region Lógica de Itens
    public void HandleSlotRightClick(InventorySlotUI clickedSlot) { if (isDragging) return; InventorySlot slotData = clickedSlot.GetLinkedSlotData(); if (slotData?.item == null || slotData.quantity <= 1 || !slotData.item.isStackable) return; if (StackSplitterUI.instance == null) { return; } StackSplitterUI.instance.Show(slotData.item, slotData.quantity, (amountToSplit) => { ConfirmSplit(clickedSlot, amountToSplit); }); }
    private void ConfirmSplit(InventorySlotUI fromSlot, int amountToSplit) { int emptySlotIndex = PlayerItens.instance.FindNextEmptyInventorySlot(); if (emptySlotIndex == -1) { return; } ItemData itemToMove = fromSlot.GetLinkedSlotData().item; PlayerItens.instance.RemoveQuantityFromSlot(fromSlot.GetContainerType(), fromSlot.GetIndex(), amountToSplit); PlayerItens.instance.AddItemToSlot(ContainerType.Inventory, emptySlotIndex, itemToMove, amountToSplit); UpdateAllVisuals(); }
    public void StartDrag(InventorySlotUI fromSlot) { if (fromSlot.GetLinkedSlotData()?.item == null || isDragging) return; sourceSlotUI = fromSlot; isDragging = true; draggedItemIcon.transform.SetParent(transform.root, true); draggedItemIcon.transform.SetAsLastSibling(); draggedItemIcon.gameObject.SetActive(true); UpdateDraggedVisuals(sourceSlotUI.GetLinkedSlotData()); }
    public void DropItemOnSlot(InventorySlotUI destinationSlotUI) { if (sourceSlotUI == null || destinationSlotUI == null) { CancelDrag(); return; } InventorySlot sourceData = sourceSlotUI.GetLinkedSlotData(); InventorySlot destinationData = destinationSlotUI.GetLinkedSlotData(); if (sourceData == null || destinationData == null) { CancelDrag(); return; } if (sourceSlotUI == destinationSlotUI) { } else if (destinationData.item == null) { destinationData.SetItem(sourceData.item, sourceData.quantity); sourceData.ClearSlot(); } else if (destinationData.item == sourceData.item && destinationData.item.isStackable) { int spaceInDestination = destinationData.item.maxStackSize - destinationData.quantity; if (spaceInDestination > 0) { int amountToMove = Mathf.Min(sourceData.quantity, spaceInDestination); destinationData.quantity += amountToMove; sourceData.quantity -= amountToMove; if (sourceData.quantity <= 0) sourceData.ClearSlot(); } else { SwapItems(sourceData, destinationData); } } else { SwapItems(sourceData, destinationData); } StopDrag(); }
    private void SwapItems(InventorySlot source, InventorySlot destination) { ItemData tempItem = destination.item; int tempQuantity = destination.quantity; destination.SetItem(source.item, source.quantity); source.SetItem(tempItem, tempQuantity); }
    public void HandleWorldDrop() { if (sourceSlotUI != null) { if (sourceSlotUI.GetContainerType() == ContainerType.Inventory || sourceSlotUI.GetContainerType() == ContainerType.Hotbar) { InventorySlot sourceData = sourceSlotUI.GetLinkedSlotData(); PlayerItens.instance.DropItemToWorld(sourceData); PlayerItens.instance.ClearSlot(sourceSlotUI.GetContainerType(), sourceSlotUI.GetIndex()); } } StopDrag(); }
    public void CancelDrag() { if (isDragging) StopDrag(); }
    public void StopDrag() { isDragging = false; if (sourceSlotUI != null) sourceSlotUI.GetComponent<CanvasGroup>().alpha = 1f; sourceSlotUI = null; if (draggedItemIcon != null) { draggedItemIcon.transform.SetParent(originalIconParent, true); draggedItemIcon.gameObject.SetActive(false); } UpdateAllVisuals(); }
    private void UpdateDraggedVisuals(InventorySlot data) { if (draggedItemIcon == null || data?.item == null) return; draggedItemIcon.sprite = data.item.icon; draggedItemIcon.color = Color.white; UpdateDragPosition(Input.mousePosition); if (draggedItemQuantityText != null) { if (data.quantity > 1) { draggedItemQuantityText.text = data.quantity.ToString(); draggedItemQuantityText.enabled = true; } else { draggedItemQuantityText.enabled = false; } } }
    private void UpdateDragPosition(Vector2 screenPosition) { if (draggedItemIcon == null || !isDragging) return; draggedItemIcon.transform.position = screenPosition; }
    public void UpdateAllVisuals() { if (InventoryManager.instance != null) InventoryManager.instance.UpdateDisplay(); if (HotbarController.instance != null) HotbarController.instance.UpdateDisplay(); if (ChestUIManager.instance != null && ChestUIManager.instance.currentChestInventory != null) { ChestUIManager.instance.UpdateDisplay(); } }
    #endregion
}