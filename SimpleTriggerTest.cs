using UnityEngine;

public class SimpleTriggerTest : MonoBehaviour
{
    // Este método é chamado pela engine da Unity sempre que algo entra no trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ele vai imprimir o nome, a tag e a layer de QUALQUER objeto que entrar
        //Debug.Log($"<color=yellow>TESTE DE TRIGGER:</color> Trigger ativado por: '{other.gameObject.name}' | Tag: '{other.tag}' | Layer: '{LayerMask.LayerToName(other.gameObject.layer)}'");
    }
}