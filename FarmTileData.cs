using UnityEngine;
using UnityEngine.Tilemaps;

// Não precisa ser um MonoBehaviour, é apenas uma classe para guardar dados.
[System.Serializable]
public class FarmTileData
{
    public enum TileState { Default, Dug, Watered }

    public Vector3Int position;
    public TileState state = TileState.Default;

    // Futuramente, adicionaremos aqui informações sobre a semente plantada e seu estágio de crescimento.
}