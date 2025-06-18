using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class HudController : MonoBehaviour
{
    // << ARRASTE SUAS BARRAS DE UI AQUI NO INSPECTOR >>
    [Header("Barras de Recursos")]
    [SerializeField] private Image woodUIBar;
    [SerializeField] private Image fishUIBar;
    [SerializeField] private Image carrotUIBar;
    // Adicione outras barras aqui se precisar

    // << ARRASTE OS ITEMDATA CORRESPONDENTES AQUI >>
    [Header("Referências de ItemData")]
    [SerializeField] private ItemData woodItemData;
    [SerializeField] private ItemData fishItemData;
    [SerializeField] private ItemData carrotItemData;
    // Adicione outros ItemData aqui

    void Update()
    {
        if (PlayerItens.instance == null) return;

        // Itera pelos recursos que o PlayerItens está rastreando
        foreach (var resource in PlayerItens.instance.trackedResources)
        {
            Image barToUpdate = null;

            // Verifica qual barra deve ser atualizada
            if (resource.itemData == woodItemData) barToUpdate = woodUIBar;
            else if (resource.itemData == fishItemData) barToUpdate = fishUIBar;
            else if (resource.itemData == carrotItemData) barToUpdate = carrotUIBar;

            // Se encontrou uma barra correspondente, atualiza o preenchimento dela
            if (barToUpdate != null)
            {
                barToUpdate.fillAmount = (float)resource.currentAmount / resource.limit;
            }
        }
    }
}