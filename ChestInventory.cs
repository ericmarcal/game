using UnityEngine;
using System.Collections.Generic;

public class ChestInventory : MonoBehaviour
{
    [Header("Configuração do Baú")]
    [Tooltip("O número de slots que este tipo de baú terá.")]
    public int chestSize = 12;

    [Header("Conteúdo do Baú")]
    public List<InventorySlot> chestSlots = new List<InventorySlot>();

    private void Awake()
    {
        // Inicializa o baú com o número correto de slots vazios
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        chestSlots.Clear();
        for (int i = 0; i < chestSize; i++)
        {
            chestSlots.Add(new InventorySlot());
        }
    }

    // Futuramente, podemos adicionar métodos aqui para salvar e carregar o conteúdo do baú.
    // Por enquanto, sua função principal é apenas armazenar os dados.
}