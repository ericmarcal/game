using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para reiniciar a cena

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Telas da UI")]
    [SerializeField] private GameObject gameOverPanel;

    private void Awake()
    {
        // Padrão Singleton simples
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
        // Garante que a tela de Game Over comece desativada
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            // Poderíamos pausar o tempo aqui se quiséssemos
            // Time.timeScale = 0f;
        }
    }

    public void RestartGame()
    {
        // Despausa o tempo se ele foi pausado
        // Time.timeScale = 1f;
        // Recarrega a cena atual
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}