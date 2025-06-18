using UnityEngine;

public enum ItemType
{
    Recurso,
    Ferramenta,
    Consumivel,
    Semente,
    Placeable // << NOVO TIPO ADICIONADO
}

[CreateAssetMenu(fileName = "New ItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Informações Básicas")]
    public string itemName = "New Item";
    [TextArea(3, 10)]
    public string description = "Descrição do item aqui...";
    public Sprite icon = null;
    public GameObject itemPrefab;

    [Header("Categorização e Comportamento")]
    public ItemType itemType = ItemType.Recurso;
    public ToolType associatedTool = ToolType.None;

    // --- NOVO CAMPO ADICIONADO AQUI ---
    [Header("Posicionável (Placeable)")]
    [Tooltip("Se o item for do tipo Placeable, este é o prefab que será instanciado no mundo.")]
    public GameObject placeablePrefab;
    // ------------------------------------

    [Header("Plantação")]
    [Tooltip("Se este item for uma semente, arraste aqui os dados da plantação correspondente.")]
    public CropData cropData;

    [Header("Consumível")]
    [Tooltip("Se marcado, este item pode ser consumido pelo jogador.")]
    public bool isConsumable = false;
    [Tooltip("A quantidade de vida (HP) que este item restaura.")]
    public float healthToRestore = 0;
    [Tooltip("A quantidade de vigor (Stamina) que este item restaura.")]
    public float staminaToRestore = 0;

    [Header("Empilhamento")]
    public bool isStackable = true;
    public int maxStackSize = 64;

    [Header("Comportamento e UI")]
    public bool isCollectible = true;
    public bool isTrackedOnHUD = false;
    public int trackedLimit = 50;
}