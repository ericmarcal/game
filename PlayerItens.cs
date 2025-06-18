using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerItens : MonoBehaviour
{
    public static PlayerItens instance;
    [System.Serializable]
    public class DebugItemToAdd { public ItemData item; public int quantity = 1; }
    [Header("Debug / Itens Iniciais")]
    public List<DebugItemToAdd> initialItems;
    public List<DebugItemToAdd> initialHotbarItems;
    [Header("Inventário e Hotbar")]
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();
    public int inventorySize = 16;
    public List<InventorySlot> hotbarSlots = new List<InventorySlot>();
    public int hotbarSize = 5;
    [Header("Recursos de Água")]
    public float curentWater;
    public float waterLimit = 50;

    [Header("Configurações de Drop")]
    [SerializeField] private float itemDropForce = 2f;
    [Tooltip("A que distância à frente do jogador o item aparece ao ser dropado.")]
    [SerializeField] private float itemDropOffset = 0.75f;
    [Tooltip("Duração da animação do item sendo 'empurrado' pelo jogador.")]
    [SerializeField] private float dropPushDuration = 0.2f;
    [Tooltip("Quais camadas são consideradas obstáculos para o drop de itens.")]
    [SerializeField] private LayerMask obstacleLayerMask;

    // O campo itemLinearDrag foi removido daqui

    [System.Serializable]
    public class TrackedResource { public ItemData itemData; public int currentAmount; public int limit; }
    [HideInInspector]
    public List<TrackedResource> trackedResources = new List<TrackedResource>();

    // ... (o resto do script continua exatamente igual) ...
    private void Awake() { if (instance == null) instance = this; else if (instance != this) { Destroy(gameObject); return; } InitializeSlots(inventorySlots, inventorySize); InitializeSlots(hotbarSlots, hotbarSize); DiscoverAndSetupTrackedResources(); }
    private void Start() { if (HotbarController.instance != null) { HotbarController.instance.InitializeHotbar(); } AddInitialItems(); AddInitialHotbarItems(); RecalculateAllTrackedResources(); if (HotbarController.instance != null) HotbarController.instance.UpdateDisplay(); if (InventoryManager.instance != null && InventoryManager.instance.IsInventoryOpen()) InventoryManager.instance.UpdateDisplay(); }
    public void DropItemToWorld(InventorySlot itemSlot) { if (itemSlot == null || itemSlot.item == null || itemSlot.item.itemPrefab == null) return; Player player = FindObjectOfType<Player>(); Vector3 playerPosition = player != null ? player.transform.position : transform.position; Vector2 dropDirection = player != null ? player.lastMoveDirection : Vector2.down; Vector3 desiredDropPosition = playerPosition + (Vector3)dropDirection.normalized * itemDropOffset; Vector3 finalDropPosition = desiredDropPosition; if (Physics2D.OverlapCircle(desiredDropPosition, 0.1f, obstacleLayerMask)) { finalDropPosition = playerPosition; } GameObject itemGO = Instantiate(itemSlot.item.itemPrefab, playerPosition, Quaternion.identity); WorldItem worldItem = itemGO.GetComponent<WorldItem>(); if (worldItem != null) { worldItem.itemData = itemSlot.item; worldItem.quantity = itemSlot.quantity; worldItem.isMagnetic = false; worldItem.pickupDelay = 1.0f; worldItem.AnimatePush(playerPosition, finalDropPosition, dropPushDuration); } }
    private void AddInitialItems() { if (initialItems == null || initialItems.Count == 0) return; foreach (var debugItem in initialItems) { if (debugItem.item != null && debugItem.quantity > 0) AddItem(debugItem.item, debugItem.quantity); } initialItems.Clear(); }
    private void AddInitialHotbarItems() { if (initialHotbarItems == null || initialHotbarItems.Count == 0) return; for (int i = 0; i < initialHotbarItems.Count && i < hotbarSize; i++) { var itemToAdd = initialHotbarItems[i]; if (itemToAdd != null && itemToAdd.item != null && itemToAdd.quantity > 0) { InventorySlot targetSlot = GetSlot(ContainerType.Hotbar, i); if (targetSlot != null && targetSlot.item == null) { AddItemToSlot(ContainerType.Hotbar, i, itemToAdd.item, itemToAdd.quantity); } } } if (HotbarController.instance != null) { HotbarController.instance.UpdateDisplay(); } initialHotbarItems.Clear(); }
    private void InitializeSlots(List<InventorySlot> slots, int size) { slots.Clear(); for (int i = 0; i < size; i++) { slots.Add(new InventorySlot()); } }
    private void DiscoverAndSetupTrackedResources() { trackedResources.Clear(); ItemData[] allItems = Resources.LoadAll<ItemData>(""); foreach (ItemData item in allItems) { if (item.isTrackedOnHUD) trackedResources.Add(new TrackedResource { itemData = item, limit = item.trackedLimit, currentAmount = 0 }); } }
    private void RecalculateAllTrackedResources() { foreach (var resource in trackedResources) RecalculateTrackedResource(resource.itemData); }
    private void RecalculateTrackedResource(ItemData item) { TrackedResource resource = trackedResources.FirstOrDefault(r => r.itemData == item); if (resource == null) return; int totalCount = 0; List<InventorySlot> allSlots = inventorySlots.Concat(hotbarSlots).ToList(); foreach (var slot in allSlots) { if (slot.item == item) { totalCount += slot.quantity; } } resource.currentAmount = Mathf.Clamp(totalCount, 0, resource.limit); }
    public int FindNextEmptyInventorySlot() { for (int i = 0; i < inventorySlots.Count; i++) { if (inventorySlots[i].item == null) return i; } return -1; }
    public void ConsumeItemInSlot(ContainerType container, int index) { Player player = FindObjectOfType<Player>(); if (player == null) return; InventorySlot slot = GetSlot(container, index); if (slot == null || slot.item == null || !slot.item.isConsumable) return; player.RestoreHealth(slot.item.healthToRestore); player.RestoreStamina(slot.item.staminaToRestore); RemoveQuantityFromSlot(container, index, 1); if (InventoryManager.instance != null) { InventoryManager.instance.UpdateAllVisuals(); } }
    public void ConsumeFirstAvailableHotbarItem() { for (int i = 0; i < hotbarSlots.Count; i++) { InventorySlot slot = hotbarSlots[i]; if (slot != null && slot.item != null && slot.item.isConsumable) { ConsumeItemInSlot(ContainerType.Hotbar, i); return; } } }
    public InventorySlot GetSlot(ContainerType type, int index) { List<InventorySlot> list = null; switch (type) { case ContainerType.Inventory: list = inventorySlots; break; case ContainerType.Hotbar: list = hotbarSlots; break; } if (list == null || index < 0 || index >= list.Count) return null; return list[index]; }
    public void AddItemToSlot(ContainerType type, int index, ItemData item, int quantity) { InventorySlot targetSlot = GetSlot(type, index); if (targetSlot == null || item == null || quantity <= 0) return; if (targetSlot.item == null) { targetSlot.SetItem(item, quantity); } else if (targetSlot.item == item) { targetSlot.quantity += quantity; } UpdateTrackedResourceCount(item, quantity); }
    public bool CanAddItem(ItemData item, int quantity = 1) { if (item == null) return false; int spaceAvailable = 0; List<InventorySlot> allSlots = inventorySlots.Concat(hotbarSlots).ToList(); foreach (var slot in allSlots) { if (slot.item == item) spaceAvailable += item.maxStackSize - slot.quantity; else if (slot.item == null) spaceAvailable += item.maxStackSize; } return spaceAvailable >= quantity; }
    public int AddItem(ItemData item, int quantity) { if (item == null || quantity <= 0) return quantity; int originalQuantity = quantity; List<InventorySlot> allSlots = inventorySlots.Concat(hotbarSlots).ToList(); foreach (var slot in allSlots) { if (quantity <= 0) break; if (slot.item == item && slot.quantity < item.maxStackSize) { int toAdd = Mathf.Min(quantity, item.maxStackSize - slot.quantity); slot.quantity += toAdd; quantity -= toAdd; } } if (quantity > 0) { foreach (var slot in inventorySlots) { if (quantity <= 0) break; if (slot.item == null) { int toAdd = Mathf.Min(quantity, item.maxStackSize); slot.SetItem(item, toAdd); quantity -= toAdd; } } } int quantityAdded = originalQuantity - quantity; if (quantityAdded > 0) UpdateTrackedResourceCount(item, quantityAdded); RecalculateAllTrackedResources(); if (InventoryManager.instance != null && InventoryManager.instance.IsInventoryOpen()) InventoryManager.instance.UpdateDisplay(); if (HotbarController.instance != null) HotbarController.instance.UpdateDisplay(); return quantity; }
    public void RemoveQuantityFromSlot(ContainerType type, int index, int quantityToRemove) { InventorySlot slot = GetSlot(type, index); if (slot == null || slot.item == null) return; if (quantityToRemove <= 0) return; UpdateTrackedResourceCount(slot.item, -quantityToRemove); slot.quantity -= quantityToRemove; if (slot.quantity <= 0) { ClearSlot(type, index); } }
    public void ClearSlot(ContainerType type, int index) { InventorySlot slot = GetSlot(type, index); if (slot == null) return; if (slot.item != null) UpdateTrackedResourceCount(slot.item, -slot.quantity); slot.ClearSlot(); }
    private void UpdateTrackedResourceCount(ItemData item, int amountChanged) { if (item == null) return; TrackedResource resource = trackedResources.FirstOrDefault(r => r.itemData == item); if (resource != null) resource.currentAmount = Mathf.Clamp(resource.currentAmount + amountChanged, 0, resource.limit); }
    public int GetItemCount(ItemData item) { int count = 0; foreach (var slot in inventorySlots.Concat(hotbarSlots)) { if (slot.item == item) { count += slot.quantity; } } return count; }
    public bool HasIngredients(List<Ingredient> ingredients) { foreach (var ingredient in ingredients) { if (GetItemCount(ingredient.item) < ingredient.quantity) { return false; } } return true; }
    public void RemoveIngredients(List<Ingredient> ingredients) { foreach (var ingredient in ingredients) { int quantityToRemove = ingredient.quantity; for (int i = 0; i < inventorySlots.Count; i++) { if (quantityToRemove > 0 && inventorySlots[i].item == ingredient.item) { int amountInSlot = inventorySlots[i].quantity; int amountToRemoveFromSlot = Mathf.Min(quantityToRemove, amountInSlot); RemoveQuantityFromSlot(ContainerType.Inventory, i, amountToRemoveFromSlot); quantityToRemove -= amountToRemoveFromSlot; } } for (int i = 0; i < hotbarSlots.Count; i++) { if (quantityToRemove > 0 && hotbarSlots[i].item == ingredient.item) { int amountInSlot = hotbarSlots[i].quantity; int amountToRemoveFromSlot = Mathf.Min(quantityToRemove, amountInSlot); RemoveQuantityFromSlot(ContainerType.Hotbar, i, amountToRemoveFromSlot); quantityToRemove -= amountToRemoveFromSlot; } } } }
}