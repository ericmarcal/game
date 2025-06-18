using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTriggerUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private ItemData itemData;

    public void Setup(ItemData data)
    {
        this.itemData = data;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemData != null)
        {
            // CORREÇÃO AQUI: Chamando o método Show() com o modo padrão
            TooltipSystem.instance.Show(itemData, TooltipSystem.TooltipPositionMode.FollowMouse);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // CORREÇÃO AQUI: Chamando o método Hide()
        TooltipSystem.instance.Hide();
    }
}