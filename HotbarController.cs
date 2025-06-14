using UnityEngine;
using System.Collections.Generic;

public class HotbarController : MonoBehaviour
{
    public static HotbarController instance;
    [SerializeField] private List<InventorySlotUI> hotbarUISlots;

    private int currentSelectedSlot = -1;

    void Awake()
    {
        instance = this;
    }

    public void InitializeHotbar()
    {
        if (hotbarUISlots == null) return;

        for (int i = 0; i < hotbarUISlots.Count; i++)
        {
            if (i < PlayerItens.instance.hotbarSlots.Count)
            {
                // << CHAMADA CORRIGIDA AQUI >>
                hotbarUISlots[i].Link(i, ContainerType.Hotbar);
            }
        }
        UpdateSelection(-1);
    }

    public void UpdateSelection(int slotIndex)
    {
        currentSelectedSlot = slotIndex;
        for (int i = 0; i < hotbarUISlots.Count; i++)
        {
            if (hotbarUISlots[i] != null)
            {
                hotbarUISlots[i].SetSelectedStyle(i == currentSelectedSlot);
            }
        }
    }

    public void UpdateDisplay()
    {
        if (hotbarUISlots == null) return;

        for (int i = 0; i < hotbarUISlots.Count; i++)
        {
            if (hotbarUISlots[i] != null)
            {
                hotbarUISlots[i].UpdateVisuals();
            }
        }
    }
}