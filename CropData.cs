using UnityEngine;

[CreateAssetMenu(fileName = "New CropData", menuName = "Farming/Crop Data")]
public class CropData : ScriptableObject
{
    [Header("Configuração da Plantação")]
    [Tooltip("Item da semente necessária para esta planta.")]
    public ItemData seedItem;

    [Tooltip("Item que será dropado na colheita.")]
    public ItemData harvestableItem;

    [Tooltip("Quantidade de itens a serem dropados na colheita.")]
    public int amountToHarvest = 1;

    [Header("Estágios de Crescimento")]
    [Tooltip("Tempo em segundos para avançar para o próximo estágio.")]
    public float timeBetweenStages = 15f;

    [Tooltip("Sprites para os 4 estágios de crescimento. O primeiro é a semente plantada.")]
    public Sprite[] growthStages = new Sprite[4];
}