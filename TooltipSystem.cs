using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem instance;
    public enum TooltipPositionMode { FollowMouse, AboveTransform }

    [Header("Referências da UI")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private LayoutElement panelLayoutElement;

    [Header("Configurações")]
    [SerializeField] private float maxWidth = 350f;
    [SerializeField] private Vector2 positionOffset = new Vector2(15f, 15f);

    private RectTransform panelRectTransform;
    private CanvasGroup canvasGroup;
    private TooltipPositionMode currentMode;
    private Transform anchorTransform;

    private void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }

        if (tooltipPanel != null)
        {
            panelRectTransform = tooltipPanel.GetComponent<RectTransform>();
            panelLayoutElement = tooltipPanel.GetComponent<LayoutElement>();
            if (panelLayoutElement == null) panelLayoutElement = tooltipPanel.AddComponent<LayoutElement>();
            canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = tooltipPanel.AddComponent<CanvasGroup>();

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            Hide(); // CORREÇÃO AQUI: Usando o novo nome do método
        }
    }

    private void LateUpdate()
    {
        if (canvasGroup != null && canvasGroup.alpha > 0)
        {
            switch (currentMode)
            {
                case TooltipPositionMode.FollowMouse:
                    PositionFollowMouse();
                    break;
                case TooltipPositionMode.AboveTransform:
                    PositionAboveTransform();
                    break;
            }
        }
    }

    private void PositionFollowMouse()
    {
        panelRectTransform.pivot = new Vector2(0, 1);
        Vector2 offset = new Vector2(positionOffset.x, -positionOffset.y);
        tooltipPanel.transform.position = (Vector2)Input.mousePosition + offset;
    }

    private void PositionAboveTransform()
    {
        if (anchorTransform == null) { PositionFollowMouse(); return; }
        panelRectTransform.pivot = new Vector2(0.5f, 0);
        tooltipPanel.transform.position = anchorTransform.position + new Vector3(0, positionOffset.y, 0);
    }

    // NOME ATUALIZADO
    public void Show(ItemData item, TooltipPositionMode mode, RectTransform anchor = null)
    {
        if (item == null) return;

        this.currentMode = mode;
        this.anchorTransform = anchor;

        if (tooltipPanel != null) tooltipPanel.SetActive(true);
        if (gameObject != null) gameObject.SetActive(true);

        titleText.text = item.itemName;
        descriptionText.text = item.description ?? "";

        float preferredWidth = Mathf.Max(titleText.preferredWidth, descriptionText.preferredWidth);
        float clampedWidth = Mathf.Min(preferredWidth, maxWidth);
        panelLayoutElement.preferredWidth = clampedWidth;

        canvasGroup.alpha = 1f;
    }

    // NOME ATUALIZADO
    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }
}