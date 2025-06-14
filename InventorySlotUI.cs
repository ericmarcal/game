using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image selectionHighlight;
    private int myIndex;
    private ContainerType myContainerType;
    private CanvasGroup canvasGroup;

    public int GetIndex() => myIndex;
    public ContainerType GetContainerType() => myContainerType;

    public InventorySlot GetLinkedSlotData()
    {
        switch (myContainerType)
        {
            case ContainerType.Inventory:
            case ContainerType.Hotbar:
                return PlayerItens.instance.GetSlot(myContainerType, myIndex);
            case ContainerType.Chest:
                if (ChestUIManager.instance != null && ChestUIManager.instance.currentChestInventory != null)
                {
                    if (myIndex < ChestUIManager.instance.currentChestInventory.chestSlots.Count)
                    {
                        return ChestUIManager.instance.currentChestInventory.chestSlots[myIndex];
                    }
                }
                return null;
            default:
                return null;
        }
    }

    private void Awake() { canvasGroup = GetComponent<CanvasGroup>(); if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>(); }

    public void Link(int index, ContainerType type) { myIndex = index; myContainerType = type; UpdateVisuals(); }

    public void OnPointerClick(PointerEventData eventData)
    {
        // << LÓGICA ATUALIZADA PARA DUPLO CLIQUE >>
        if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount == 2)
        {
            // Tenta consumir o item com duplo clique, independentemente de onde ele esteja
            PlayerItens.instance.ConsumeItemInSlot(myContainerType, myIndex);
        }
        // Lógica de dividir o stack com clique direito (sem shift)
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (myContainerType != ContainerType.Chest)
            {
                InventoryManager.instance.HandleSlotRightClick(this);
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData) { if (eventData.button != PointerEventData.InputButton.Left || GetLinkedSlotData()?.item == null || InventoryManager.instance.IsDragging()) return; TooltipManager.instance.HideTooltip(); InventoryManager.instance.StartDrag(this); canvasGroup.alpha = 0.5f; canvasGroup.blocksRaycasts = false; }
    public void OnDrag(PointerEventData eventData) { }
    public void OnDrop(PointerEventData eventData) { if (!InventoryManager.instance.IsDragging()) return; InventoryManager.instance.DropItemOnSlot(this); }
    public void OnEndDrag(PointerEventData eventData) { canvasGroup.alpha = 1f; canvasGroup.blocksRaycasts = true; if (InventoryManager.instance.IsDragging()) { if (eventData.pointerCurrentRaycast.gameObject == null) { InventoryManager.instance.HandleWorldDrop(); } else { InventoryManager.instance.CancelDrag(); } } }
    public void OnPointerEnter(PointerEventData eventData) { if (InventoryManager.instance.IsDragging()) return; ItemData itemInSlot = GetLinkedSlotData()?.item; if (itemInSlot != null) { TooltipManager.instance.ShowTooltip(itemInSlot); } }
    public void OnPointerExit(PointerEventData eventData) { TooltipManager.instance.HideTooltip(); }
    public void UpdateVisuals() { InventorySlot slotData = GetLinkedSlotData(); if (icon == null) return; bool hasItem = slotData != null && slotData.item != null; if (hasItem) { icon.sprite = slotData.item.icon; icon.color = Color.white; quantityText.enabled = slotData.quantity > 1; quantityText.text = slotData.quantity.ToString(); } else { icon.sprite = null; icon.color = Color.clear; quantityText.enabled = false; } }
    public void SetSelectedStyle(bool isSelected) { if (myContainerType != ContainerType.Hotbar) return; if (selectionHighlight != null) { selectionHighlight.enabled = isSelected; } canvasGroup.alpha = isSelected ? 1f : 0.7f; }
}