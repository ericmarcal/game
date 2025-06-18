using UnityEngine;
using UnityEngine.Tilemaps;

public class IslandGenerator : MonoBehaviour
{
    [Header("Referências do Tilemap")]
    [Tooltip("Arraste para cá o Tilemap onde a ilha/lago será desenhado.")]
    [SerializeField] private Tilemap islandTilemap;

    [Header("Assets dos Tiles")]
    [Tooltip("O tile de Grama para o terreno principal.")]
    [SerializeField] private TileBase grassTile;
    [Tooltip("O tile de Areia para as praias.")]
    [SerializeField] private TileBase sandTile;
    [Tooltip("O tile de Água (idealmente um Rule Tile).")]
    [SerializeField] private TileBase waterTile;

    [Header("Modo de Geração")]
    [Tooltip("Marque esta caixa para gerar um lago em um continente, em vez de uma ilha em um oceano.")]
    [SerializeField] private bool generateLakeInsteadOfIsland = false;

    [Header("Dimensões do Mapa")]
    [SerializeField] private int mapWidth = 128;
    [SerializeField] private int mapHeight = 128;

    [Header("Configurações do Ruído (Noise)")]
    [SerializeField] private float noiseScale = 20f;
    [SerializeField] private int seed = 0;
    [SerializeField] private float offsetX = 100f;
    [SerializeField] private float offsetY = 100f;

    [Header("Limites do Terreno (Thresholds)")]
    [Range(0f, 1f)]
    [SerializeField] private float landThreshold = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float sandThreshold = 0.45f;

    [ContextMenu("Generate Island/Lake")]
    public void Generate()
    {
        if (islandTilemap == null || waterTile == null || sandTile == null || grassTile == null)
        {
            //Debug.LogError("Por favor, atribua todos os Tiles no Inspector antes de gerar!", this);
            return;
        }

        float[,] noiseMap = GenerateNoiseMap();
        islandTilemap.ClearAllTiles();

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float currentHeight = noiseMap[x, y];
                TileBase tileToPlace;

                if (generateLakeInsteadOfIsland)
                {
                    // Lógica para gerar um LAGO
                    if (currentHeight > landThreshold) tileToPlace = waterTile;
                    else if (currentHeight > sandThreshold) tileToPlace = sandTile;
                    else tileToPlace = grassTile;
                }
                else
                {
                    // Lógica para gerar uma ILHA
                    if (currentHeight > landThreshold) tileToPlace = grassTile;
                    else if (currentHeight > sandThreshold) tileToPlace = sandTile;
                    else tileToPlace = waterTile;
                }
                islandTilemap.SetTile(new Vector3Int(x, y, 0), tileToPlace);
            }
        }
    }

    private float[,] GenerateNoiseMap()
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];
        System.Random prng = new System.Random(seed);
        float finalOffsetX = (seed == 0) ? prng.Next(-10000, 10000) : offsetX;
        float finalOffsetY = (seed == 0) ? prng.Next(-10000, 10000) : offsetY;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float sampleX = (x + finalOffsetX) / noiseScale;
                float sampleY = (y + finalOffsetY) / noiseScale;
                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                noiseMap[x, y] = perlinValue;
            }
        }
        return noiseMap;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(new Vector3(mapWidth / 2, mapHeight / 2, 0), new Vector3(mapWidth, mapHeight, 1));
    }
}