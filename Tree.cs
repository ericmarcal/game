using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class Tree : MonoBehaviour
{
    [Header("Vida da Árvore")]
    [SerializeField] private float treehealth = 3f;
    [SerializeField] private float maxHealth = 3f;

    [Header("Regeneração")]
    [SerializeField] private float regrowDelay = 5f;

    [Header("Configurações do Drop")]
    [Tooltip("O prefab do item de madeira que será dropado (DEVE ter o script WorldItem.cs).")]
    [SerializeField] private GameObject woodWorldItemPrefab;
    [Tooltip("O ItemData correspondente à madeira.")]
    [SerializeField] private ItemData woodItemData;
    [Tooltip("A força com que a madeira 'pula' ao ser dropada.")]
    [SerializeField] private float dropPopForce = 2.0f; // << NOVO

    [SerializeField] private float dropOffsetY = 0.5f;
    [SerializeField] private float dropRadius = 0.7f;
    [SerializeField] private int minWoodDrop = 1;
    [SerializeField] private int maxWoodDrop = 3;

    [Header("Efeitos Visuais")]
    [SerializeField] private ParticleSystem leafs;

    private Animator anim;
    private bool isCut;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        //if (woodWorldItemPrefab == null) Debug.LogError($"Tree.cs ({gameObject.name}): woodWorldItemPrefab não atribuído!", this);
        //if (woodItemData == null) Debug.LogError($"Tree.cs ({gameObject.name}): woodItemData (ScriptableObject da Madeira) não atribuído!", this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(GameTags.Axe) && !isCut)
        {
            OnHit();
        }
    }

    public void OnHit(int damageAmount = 1)
    {
        if (isCut || treehealth <= 0) return;
        treehealth -= damageAmount;
        anim.SetTrigger("isHit");
        if (leafs != null) leafs.Play();

        if (treehealth <= 0)
        {
            isCut = true;
            anim.SetTrigger("cut");
            DropWood();
            Invoke(nameof(RegrowTree), regrowDelay);
        }
    }

    private void DropWood()
    {
        if (woodWorldItemPrefab == null || woodItemData == null) return;
        int amountToDrop = Random.Range(minWoodDrop, maxWoodDrop + 1);

        for (int i = 0; i < amountToDrop; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * dropRadius;
            Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y - dropOffsetY, 0f);

            GameObject woodInstance = Instantiate(woodWorldItemPrefab, spawnPosition, Quaternion.identity);
            WorldItem worldItemScript = woodInstance.GetComponent<WorldItem>();
            if (worldItemScript != null)
            {
                worldItemScript.itemData = woodItemData;
                worldItemScript.quantity = 1;

                // << CHAMADA ATUALIZADA >>
                Vector2 forceDirection = new Vector2(Random.Range(-0.5f, 0.5f), 1f);
                worldItemScript.SetupSpawnedItemParameters(spawnPosition, forceDirection, dropPopForce);
            }
            else
            {
                //Debug.LogError($"Tree.cs: Prefab '{woodWorldItemPrefab.name}' não contém o script WorldItem!", woodInstance);
            }
        }
    }

    private void RegrowTree()
    {
        anim.SetTrigger("regrow");
        treehealth = maxHealth;
        isCut = false;
    }
}