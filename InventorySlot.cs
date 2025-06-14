[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int quantity;

    public InventorySlot() { ClearSlot(); }

    public InventorySlot(ItemData item, int quantity)
    {
        SetItem(item, quantity);
    }

    public void SetItem(ItemData newItem, int newQuantity) { item = newItem; quantity = newQuantity; }
    public void ClearSlot() { item = null; quantity = 0; }
}