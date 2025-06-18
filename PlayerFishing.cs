using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Player))]
public class PlayerFishing : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Tilemap waterTilemap;

    [Header("Configurações de Pesca")]
    [SerializeField] private int fishingPercentage = 30;

    [Header("Configurações de Coleta de Água")]
    [SerializeField] private int waterValue = 10;
    [SerializeField] private float waterCooldown = 0.5f;

    [Header("Prefab e Item do Peixe")]
    [SerializeField] private GameObject fishWorldItemPrefab;
    [SerializeField] private ItemData fishItemData;
    [SerializeField] private float fishPopForce = 2f;

    private Player player;
    private float lastWaterCollectionTime;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    public void StartFishingAction()
    {
        if (player.grid == null) return;

        Vector3Int targetCell = player.grid.WorldToCell(player.playerActionPoint.position);
        if (waterTilemap != null && waterTilemap.HasTile(targetCell))
        {
            player.isFishing = true;
            player.canCatchFish = (Random.Range(1, 101) <= fishingPercentage);
        }
        else
        {
            player.isFishing = true;
            player.canCatchFish = false;
        }
    }

    public void SpawnFishIfCaught()
    {
        if (player.canCatchFish)
        {
            InstantiateFish();
            player.canCatchFish = false;
        }
    }

    private void InstantiateFish()
    {
        if (fishWorldItemPrefab == null || fishItemData == null) return;

        GameObject fishInstance = Instantiate(fishWorldItemPrefab, player.playerActionPoint.position, Quaternion.identity);
        WorldItem worldItemScript = fishInstance.GetComponent<WorldItem>();

        if (worldItemScript != null)
        {
            worldItemScript.itemData = fishItemData;
            worldItemScript.quantity = 1;
            Vector2 popDirection = new Vector2(Random.Range(-0.2f, 0.2f), 1f);

            // << CHAMADA ATUALIZADA >>: Agora passamos a força (fishPopForce) para o método.
            worldItemScript.SetupSpawnedItemParameters(player.playerActionPoint.position, popDirection, fishPopForce);
        }
    }

    public void TryToCollectWater()
    {
        if (Time.time - lastWaterCollectionTime < waterCooldown || player.grid == null) return;

        Vector3Int targetCell = player.grid.WorldToCell(player.playerActionPoint.position);
        if (waterTilemap != null && waterTilemap.HasTile(targetCell))
        {
            lastWaterCollectionTime = Time.time;
            PlayerItens.instance.curentWater = Mathf.Min(PlayerItens.instance.curentWater + waterValue, PlayerItens.instance.waterLimit);
        }
    }
}