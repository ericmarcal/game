using UnityEngine;

public class FishingPoint : MonoBehaviour
{
    [Header("Configuração da Pesca")]
    [Tooltip("O prefab do peixe que será instanciado (DEVE ter o script WorldItem.cs).")]
    public GameObject fishWorldItemPrefab;
    [Tooltip("O ItemData correspondente ao peixe que este ponto de pesca fornece.")]
    [SerializeField] private ItemData fishItemDataToDrop;
    [Tooltip("A força com que o peixe 'pula' da água.")]
    [SerializeField] private float fishPopForce = 2f; // << NOVO
    private Player playerScript;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(GameTags.Player);
        if (playerObj != null) playerScript = playerObj.GetComponent<Player>();
        //if (playerScript == null) Debug.LogError("FishingPoint: Script Player não pôde ser encontrado no Start!", this);
        //if (fishWorldItemPrefab == null) Debug.LogError($"FishingPoint ({gameObject.name}): fishWorldItemPrefab não atribuído!", this);
        //if (fishItemDataToDrop == null) Debug.LogError($"FishingPoint ({gameObject.name}): fishItemDataToDrop não atribuído!", this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playerScript == null) return;
        if (other.CompareTag(GameTags.Water))
        {
            if (playerScript.canCatchFish)
            {
                InstantiateFish();
                playerScript.canCatchFish = false;
            }
        }
    }

    private void InstantiateFish()
    {
        if (fishWorldItemPrefab == null || fishItemDataToDrop == null) return;

        Vector3 spawnPositionAnzol = transform.position;
        GameObject fishInstance = Instantiate(fishWorldItemPrefab, spawnPositionAnzol, Quaternion.identity);
        WorldItem worldItemScript = fishInstance.GetComponent<WorldItem>();

        if (worldItemScript != null)
        {
            worldItemScript.itemData = fishItemDataToDrop;
            worldItemScript.quantity = 1;

            // << CHAMADA ATUALIZada E CORRIGIDA >>
            Vector2 popDirection = new Vector2(Random.Range(-0.2f, 0.2f), 1f);
            worldItemScript.SetupSpawnedItemParameters(spawnPositionAnzol, popDirection, fishPopForce);
        }
        //else { Debug.LogError($"FishingPoint: Prefab '{fishWorldItemPrefab.name}' não contém o script WorldItem!", fishInstance); }
    }
}