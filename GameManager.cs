using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Prefabs e Efeitos")]
    public GameObject playerPrefab;
    public GameObject transformationEffectPrefab;

    [Header("Pontos de Spawn")]
    public Transform playerSpawnPoint;

    // Referência para o script da câmera
    private CameraFollow mainCameraFollow;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Ao iniciar, tenta encontrar o script da câmera na Main Camera
        if (Camera.main != null)
        {
            mainCameraFollow = Camera.main.GetComponent<CameraFollow>();
        }
    }

    public void TransformPlayerIntoEnemy(Player player, GameObject enemyPrefab)
    {
        if (player == null || enemyPrefab == null) return;

        Vector3 spawnPosition = player.transform.position;

        if (transformationEffectPrefab != null)
        {
            Instantiate(transformationEffectPrefab, spawnPosition, Quaternion.identity);
        }

        Destroy(player.gameObject);

        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        spawnedEnemy.AddComponent<PossessedEnemyController>();

        // << LÓGICA DA CÂMERA >>
        // Se encontrou o script da câmera, define o novo alvo
        if (mainCameraFollow != null)
        {
            mainCameraFollow.SetTarget(spawnedEnemy.transform);
        }
    }

    private void TriggerGameOver()
    {
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowGameOverScreen();
        }
    }
}