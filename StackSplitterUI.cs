using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StackSplitterUI : MonoBehaviour
{
    public static StackSplitterUI instance;

    // << CAMPOS ATUALIZADOS PARA CORRESPONDER À SUA HIERARQUIA >>
    [Header("Componentes da UI")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Slider quantitySlider;
    [SerializeField] private TMP_InputField quantityInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton; // << NOVO >>
    [SerializeField] private Image iconImage;     // << NOVO >>


    private Action<int> onConfirmSplit;
    private int maxQuantity;

    private void Awake()
    {
        instance = this;
        if (mainPanel != null) mainPanel.SetActive(false);

        // Adiciona listeners para os eventos da UI
        quantitySlider.onValueChanged.AddListener(OnSliderChanged);
        quantityInputField.onValueChanged.AddListener(OnInputChanged);
        confirmButton.onClick.AddListener(OnConfirmClick);

        // << NOVO >> Adiciona o listener para o botão de cancelar
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClick);
        }
    }

    // << MÉTODO SHOW ATUALIZADO >>
    public void Show(ItemData item, int maxQty, Action<int> confirmCallback)
    {
        mainPanel.SetActive(true);
        onConfirmSplit = confirmCallback;
        maxQuantity = maxQty - 1;

        // Configura os textos e o ícone
        itemNameText.text = $"Dividir {item.itemName}";
        iconImage.sprite = item.icon; // << NOVO >>
        iconImage.color = Color.white;  // << NOVO >> Garante que a imagem esteja visível

        // Garante que o slider não tenha um valor máximo inválido
        if (maxQuantity < 1) maxQuantity = 1;

        // Configura o slider
        quantitySlider.minValue = 1;
        quantitySlider.maxValue = maxQuantity;
        quantitySlider.value = 1;

        // Configura o Input Field
        quantityInputField.text = "1";
    }

    private void OnSliderChanged(float value)
    {
        quantityInputField.text = Mathf.RoundToInt(value).ToString();
    }

    private void OnInputChanged(string value)
    {
        if (int.TryParse(value, out int amount))
        {
            amount = Mathf.Clamp(amount, 1, maxQuantity);

            // Remove o listener temporariamente para evitar um loop infinito de chamadas
            quantitySlider.onValueChanged.RemoveListener(OnSliderChanged);
            quantitySlider.value = amount;
            quantitySlider.onValueChanged.AddListener(OnSliderChanged);

            // Se o usuário digitar um valor diferente, atualiza o texto para o valor corrigido
            if (quantityInputField.text != amount.ToString())
            {
                quantityInputField.text = amount.ToString();
            }
        }
    }

    public void OnConfirmClick()
    {
        if (int.TryParse(quantityInputField.text, out int amount))
        {
            amount = Mathf.Clamp(amount, 1, maxQuantity);
            onConfirmSplit?.Invoke(amount);
        }
        Hide();
    }

    public void OnCancelClick()
    {
        Hide();
    }

    private void Hide()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        onConfirmSplit = null;
    }
}