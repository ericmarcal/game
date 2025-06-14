using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager instance;

    [Header("Referências da UI")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private VerticalLayoutGroup layoutGroup;

    [Header("Configurações de Tamanho e Posição")]
    [Tooltip("A largura máxima que o painel pode atingir.")]
    [SerializeField] private float maxWidth = 350f;
    [Tooltip("O deslocamento do tooltip em relação ao mouse.")]
    [SerializeField] private Vector2 positionOffset = new Vector2(15f, -15f);

    private RectTransform tooltipRectTransform;
    private CanvasGroup tooltipCanvasGroup;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        if (tooltipPanel != null)
        {
            tooltipRectTransform = tooltipPanel.GetComponent<RectTransform>();
            tooltipCanvasGroup = tooltipPanel.GetComponent<CanvasGroup>();

            tooltipPanel.SetActive(true);
            tooltipCanvasGroup.alpha = 0f;
            tooltipCanvasGroup.interactable = false;
            tooltipCanvasGroup.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        if (tooltipCanvasGroup != null && tooltipCanvasGroup.alpha > 0)
        {
            Vector2 mousePosition = Input.mousePosition;

            float pivotX = mousePosition.x / Screen.width < 0.5f ? 0 : 1;
            float pivotY = mousePosition.y / Screen.height < 0.5f ? 0 : 1;
            tooltipRectTransform.pivot = new Vector2(pivotX, pivotY);

            tooltipPanel.transform.position = mousePosition + positionOffset;
        }
    }

    public void ShowTooltip(ItemData item)
    {
        // << VERIFICAÇÃO DE SEGURANÇA ADICIONADA >>
        if (tooltipPanel == null || titleText == null || descriptionText == null || layoutGroup == null)
        {
            Debug.LogError("TooltipManager não está configurado corretamente! Verifique todas as referências no Inspector.");
            return;
        }

        if (item == null || string.IsNullOrEmpty(item.description))
        {
            return;
        }

        titleText.text = item.itemName;
        descriptionText.text = item.description;

        float preferredWidth = Mathf.Max(titleText.preferredWidth, descriptionText.preferredWidth);
        float totalPadding = layoutGroup.padding.left + layoutGroup.padding.right;
        float newWidth = Mathf.Min(preferredWidth + totalPadding, maxWidth);

        tooltipRectTransform.sizeDelta = new Vector2(newWidth, tooltipRectTransform.sizeDelta.y);

        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRectTransform);
        Canvas.ForceUpdateCanvases();

        float titleHeight = titleText.preferredHeight;
        float descriptionHeight = descriptionText.preferredHeight;
        float totalHeight = titleHeight + descriptionHeight + layoutGroup.spacing + layoutGroup.padding.top + layoutGroup.padding.bottom;

        tooltipRectTransform.sizeDelta = new Vector2(newWidth, totalHeight);

        tooltipCanvasGroup.alpha = 1f;
    }

    public void HideTooltip()
    {
        if (tooltipCanvasGroup != null)
        {
            tooltipCanvasGroup.alpha = 0f;
        }
    }
}