using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class TrashSlot : MonoBehaviour, IDropHandler
{
    [Header("Efeito de Balanço")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeAngle = 15f;
    [SerializeField] private float shakeSpeed = 50f;

    private RectTransform rectTransform;
    private Coroutine currentShakeCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!InventoryManager.instance.IsDragging()) return;

        // << LÓGICA CORRIGIDA AQUI >>

        // 1. Pega a referência do slot de onde o item está vindo.
        InventorySlotUI sourceSlot = InventoryManager.instance.GetSourceSlotUI();
        if (sourceSlot != null)
        {
            // 2. Chama a função para limpar os dados daquele slot no inventário do jogador.
            PlayerItens.instance.ClearSlot(sourceSlot.GetContainerType(), sourceSlot.GetIndex());
        }

        // 3. Para a ação de arrastar (o que faz o ícone do mouse sumir).
        InventoryManager.instance.StopDrag();

        // 4. Inicia o efeito visual da lixeira.
        if (currentShakeCoroutine != null) StopCoroutine(currentShakeCoroutine);
        currentShakeCoroutine = StartCoroutine(RockingCoroutine());
    }

    private IEnumerator RockingCoroutine()
    {
        float elapsedTime = 0f;
        Quaternion startRotation = rectTransform.localRotation;
        while (elapsedTime < shakeDuration)
        {
            float zRotation = Mathf.Sin(elapsedTime * shakeSpeed) * shakeAngle;
            rectTransform.localRotation = startRotation * Quaternion.Euler(0, 0, zRotation);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        rectTransform.localRotation = startRotation;
    }
}