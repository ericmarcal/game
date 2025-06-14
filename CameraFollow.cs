using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Configuração do Alvo")]
    [Tooltip("O alvo que a câmera deve seguir.")]
    public Transform target;

    [Header("Configurações de Movimento")]
    [Tooltip("Quão suavemente a câmera segue o alvo. Valores menores resultam em um movimento mais lento e suave.")]
    [Range(0.01f, 1.0f)]
    public float smoothSpeed = 0.125f;
    [Tooltip("A distância que a câmera mantém do alvo. Para um jogo 2D, o Z deve ser negativo (ex: -10).")]
    public Vector3 offset = new Vector3(0, 0, -10);

    // LateUpdate é chamado depois de todos os Updates, o que é ideal para câmeras
    // para evitar movimentos "tremidos".
    void LateUpdate()
    {
        if (target != null)
        {
            // Posição desejada pela câmera (posição do alvo + distância)
            Vector3 desiredPosition = target.position + offset;
            // Interpola suavemente da posição atual para a posição desejada
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            // Aplica a nova posição à câmera
            transform.position = smoothedPosition;
        }
    }

    // Método público para permitir que outros scripts (como o GameManager) mudem o alvo da câmera
    public void SetTarget(Transform newTarget)
    {
        this.target = newTarget;
    }
}