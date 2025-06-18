using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

// Garante que o objeto sempre tenha um PolygonCollider2D, evitando erros.
[RequireComponent(typeof(PolygonCollider2D))]
public class ResourceSpawner : MonoBehaviour
{
    [Header("Configuração do Spawn")]
    [Tooltip("O prefab do recurso que será instanciado.")]
    public GameObject resourcePrefab;
    [Tooltip("Quantos recursos tentar instanciar nesta área.")]
    public int numberOfResourcesToSpawn = 10;
    [Tooltip("Distância mínima que deve haver entre cada recurso instanciado.")]
    public float minDistanceBetweenSpawns = 1.0f;
    [Tooltip("Máximo de tentativas para encontrar uma posição válida para cada recurso.")]
    public int maxPlacementAttemptsPerResource = 20;

    [Header("Validação de Posição")]
    [Tooltip("O Tilemap onde os recursos podem ser spawnados (ex: Tilemap de Chão/Grama).")]
    public Tilemap spawnableTilemap;
    [Tooltip("Opcional: Tilemap de Água (ou outros terrenos proibidos).")]
    public Tilemap waterTilemap;
    [Tooltip("Camadas que são consideradas obstáculos.")]
    public LayerMask obstacleLayerMask;
    [Tooltip("Raio da checagem de overlap para obstáculos.")]
    public float overlapCheckRadius = 0.4f;

    [Header("Organização")]
    [Tooltip("Opcional: Um GameObject pai para os recursos instanciados.")]
    public Transform spawnedResourcesParent;

    // << MUDANÇA: Referência para a nova área de spawn >>
    private PolygonCollider2D spawnArea;
    private List<Vector3> spawnedPositions = new List<Vector3>();

    // Usamos Awake para garantir que a referência seja pega antes do Start
    private void Awake()
    {
        spawnArea = GetComponent<PolygonCollider2D>();
        // Garante que o colisor não interfira com a física do jogo, ele é só um "molde".
        spawnArea.isTrigger = true;
    }

    void Start()
    {
        if (spawnedResourcesParent == null)
        {
            spawnedResourcesParent = this.transform;
        }
        SpawnResources();
    }

    public void SpawnResources()
    {
        if (resourcePrefab == null)
        {
            //Debug.LogError($"ResourceSpawner em '{gameObject.name}': Nenhum resourcePrefab atribuído!", this);
            return;
        }
        if (spawnArea == null)
        {
            //Debug.LogError($"ResourceSpawner em '{gameObject.name}': Nenhum PolygonCollider2D encontrado! Adicione um para definir a área.", this);
            return;
        }

        spawnedPositions.Clear();
        Bounds bounds = spawnArea.bounds; // Pega os limites do nosso polígono para gerar pontos
        int spawnedCount = 0;

        for (int i = 0; i < numberOfResourcesToSpawn; i++)
        {
            for (int attempt = 0; attempt < maxPlacementAttemptsPerResource; attempt++)
            {
                // Gera um ponto aleatório dentro do retângulo que envolve o polígono
                float randomX = Random.Range(bounds.min.x, bounds.max.x);
                float randomY = Random.Range(bounds.min.y, bounds.max.y);
                Vector2 potentialSpawnPosition = new Vector2(randomX, randomY);

                // << LÓGICA PRINCIPAL: Checa se o ponto está DENTRO da forma desenhada >>
                if (spawnArea.OverlapPoint(potentialSpawnPosition))
                {
                    // Se o ponto está dentro do polígono, faz as outras validações (distância, tile, etc.)
                    if (IsValidSpawnLocation(potentialSpawnPosition))
                    {
                        Instantiate(resourcePrefab, potentialSpawnPosition, Quaternion.identity, spawnedResourcesParent);
                        spawnedPositions.Add(potentialSpawnPosition);
                        spawnedCount++;
                        break; // Posição válida encontrada, vai para o próximo recurso
                    }
                }
            }
        }
        //Debug.Log($"ResourceSpawner '{gameObject.name}': Spawn concluído. {spawnedCount}/{numberOfResourcesToSpawn} instâncias criadas.");
    }

    bool IsValidSpawnLocation(Vector3 position)
    {
        if (spawnableTilemap != null)
        {
            Vector3Int cellPosition = spawnableTilemap.WorldToCell(position);
            if (!spawnableTilemap.HasTile(cellPosition) || (waterTilemap != null && waterTilemap.HasTile(waterTilemap.WorldToCell(position))))
            {
                return false;
            }
        }
        if (Physics2D.OverlapCircle(position, overlapCheckRadius, obstacleLayerMask))
        {
            return false;
        }
        if (minDistanceBetweenSpawns > 0)
        {
            foreach (Vector3 existingPos in spawnedPositions)
            {
                if (Vector3.Distance(position, existingPos) < minDistanceBetweenSpawns)
                {
                    return false;
                }
            }
        }
        return true;
    }

    // O Gizmo foi removido pois o próprio PolygonCollider2D já tem uma excelente visualização no editor.
}