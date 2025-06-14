using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlantedCrop
{
    public CropData cropData;
    public int currentStage = 0;
    public float growthTimer;
    public Vector3Int position;
    public bool isWatered = false;

    public PlantedCrop(CropData data, Vector3Int pos)
    {
        cropData = data;
        position = pos;
        growthTimer = data.timeBetweenStages;
    }

    public void Grow()
    {
        if (!isWatered || currentStage >= cropData.growthStages.Length - 1)
        {
            return;
        }

        growthTimer -= Time.deltaTime;
        if (growthTimer <= 0)
        {
            growthTimer = cropData.timeBetweenStages;
            currentStage++;
            FarmingManager.instance.UpdatePlantSprite(position, cropData.growthStages[currentStage]);
            isWatered = false;
            FarmingManager.instance.UpdateWateredSprite(position, null);
        }
    }
}

public class FarmingManager : MonoBehaviour
{
    public static FarmingManager instance;

    [Header("Referências de Tilemap")]
    [SerializeField] private Tilemap soilTilemap;
    [SerializeField] private Tilemap plantsTilemap;
    [SerializeField] private Tilemap waterEffectTilemap;

    [Header("Referências de Tiles")]
    [SerializeField] private TileBase tilledSoilTile;
    [SerializeField] private TileBase wateredTile;

    private Dictionary<Vector3Int, PlantedCrop> plantedCrops = new Dictionary<Vector3Int, PlantedCrop>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        foreach (var crop in plantedCrops.Values)
        {
            crop.Grow();
        }
    }

    public void Harvest(Vector3Int position)
    {
        if (!plantedCrops.ContainsKey(position)) return;

        PlantedCrop cropToHarvest = plantedCrops[position];

        if (cropToHarvest.currentStage >= cropToHarvest.cropData.growthStages.Length - 1)
        {
            CropData data = cropToHarvest.cropData;
            if (data.harvestableItem == null || data.harvestableItem.itemPrefab == null) return;

            // Instancia o objeto
            GameObject itemGO = Instantiate(data.harvestableItem.itemPrefab, position + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);

            // << VERIFICAÇÃO FINAL E DETALHADA >>
            if (itemGO == null)
            {
                Debug.LogError($"FALHA CRÍTICA: Instantiate() foi chamado para o prefab '{data.harvestableItem.itemPrefab.name}' mas retornou um objeto nulo! O prefab pode estar corrompido.");
                return;
            }

            WorldItem worldItemScript = itemGO.GetComponent<WorldItem>();
            if (worldItemScript == null)
            {
                Debug.LogError($"FALHA CRÍTICA: O prefab '{itemGO.name}' foi instanciado, mas ele NÃO TEM o script 'WorldItem.cs' anexado!");
                Destroy(itemGO); // Destrói o objeto criado para não deixar lixo na cena
                return;
            }

            Debug.Log($"<color=green>SUCESSO: Colheita concluída e item '{itemGO.name}' gerado no mundo!</color>");
            worldItemScript.SetupSpawnedItemParameters(position + new Vector3(0.5f, 0.5f, 0), Vector2.up, 2f);

            UpdatePlantSprite(position, null);
            UpdateWateredSprite(position, null);
            plantedCrops.Remove(position);
        }
    }

    // ... (resto dos métodos sem alteração) ...
    public void Dig(Vector3Int position) { if (soilTilemap.GetTile(position) == tilledSoilTile || plantedCrops.ContainsKey(position)) { return; } soilTilemap.SetTile(position, tilledSoilTile); }
    public void Water(Vector3Int position) { if (plantedCrops.ContainsKey(position) && !plantedCrops[position].isWatered) { plantedCrops[position].isWatered = true; UpdateWateredSprite(position, wateredTile); } }
    public void Plant(Vector3Int position, CropData cropToPlant) { if (soilTilemap.GetTile(position) != tilledSoilTile || plantedCrops.ContainsKey(position)) { return; } PlantedCrop newPlant = new PlantedCrop(cropToPlant, position); plantedCrops.Add(position, newPlant); UpdatePlantSprite(position, newPlant.cropData.growthStages[0]); PlayerItens.instance.RemoveQuantityFromSlot(ContainerType.Hotbar, FindObjectOfType<Player>().currentHotbarIndex, 1); InventoryManager.instance.UpdateAllVisuals(); }
    public void UpdatePlantSprite(Vector3Int position, Sprite newSprite) { plantsTilemap.SetTile(position, null); if (newSprite != null) { Tile tile = ScriptableObject.CreateInstance<Tile>(); tile.sprite = newSprite; plantsTilemap.SetTile(position, tile); } }
    public void UpdateWateredSprite(Vector3Int position, TileBase tile) { waterEffectTilemap.SetTile(position, tile); }
    public bool CanHarvest(Vector3Int position) { if (!plantedCrops.ContainsKey(position)) return false; return plantedCrops[position].currentStage >= plantedCrops[position].cropData.growthStages.Length - 1; }
    public bool IsTilledSoil(Vector3Int position) { return soilTilemap.GetTile(position) == tilledSoilTile; }
}