using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour, IDamageable
{
    // ... todo o código do Player.cs ...
    // O método LateUpdate() que adicionamos foi REMOVIDO deste script.
    // O resto do código permanece exatamente igual à última versão funcional.

    #region Variáveis
    [Header("Velocidade")]
    public float speed;
    public float runSpeed;
    [Header("Atributos do Jogador")]
    public float maxHealth = 10f;
    public float currentHealth;
    public float maxStamina = 100f;
    public float currentStamina;
    [Header("Configuração de Morte")]
    [SerializeField] private float deathSequenceDelay = 2f;
    [Header("Custos de Vigor (Stamina)")]
    [SerializeField] private float runStaminaCostPerSecond = 10f;
    [SerializeField] private float rollStaminaCost = 25f;
    [SerializeField] private float staminaRegenRate = 15f;
    [Header("Efeitos de Status")]
    [Range(0.1f, 1f)]
    [SerializeField] private float slowEffectMultiplier = 0.5f;
    [Header("Teclas de Ação")]
    [SerializeField] private KeyCode useItemKey = KeyCode.F;
    [SerializeField] private KeyCode secondaryActionKey = KeyCode.E;
    [Header("Combate e Dano")]
    [SerializeField] private float invulnerabilityDuration = 1.5f;
    public Transform playerActionPoint;
    [SerializeField] private float playerActionRadius = 0.5f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float playerAttackDamage = 1f;
    [SerializeField] private float comboChainWindowDuration = 0.5f;
    [Header("Rolagem")]
    [SerializeField] private float rollSpeed = 8f;
    [Header("Referências do Mundo")]
    public Grid grid;
    [Header("Posicionamento de Itens")]
    [SerializeField] private LayerMask placementObstacleLayerMask;
    [Tooltip("A que distância do jogador o item 'fantasma' deve aparecer.")]
    [SerializeField] private float placementDistance = 1f;
    private bool _isDead = false;
    private bool _isRunning, _isRolling, _isCutting, _isDigging, _isWatering, _isMining, _isAttacking = false, isInvulnerable = false;
    public bool isFishing, isPaused, canCatchFish = false;
    private Rigidbody2D rig;
    private PlayerAnim playerAnimationController;
    private PlayerFishing playerFishing;
    private SpriteRenderer spriteRenderer;
    private Color originalSpriteColor;
    private Vector2 _direction;
    public Vector2 lastMoveDirection = Vector2.down;
    public ToolType currentTool = ToolType.None;
    [HideInInspector] public int currentHotbarIndex = 0;
    private Vector2 rollDirection;
    private float initialSpeed;
    private int attackComboCount = 0;
    private bool canChainToNextCombo = false;
    private float lastChainInputTime = 0f;
    private int activeSlowsCount = 0;
    private float speedMultiplier = 1f;
    private Coroutine damageFlashCoroutine;
    private Coroutine invulnerabilityCoroutine;
    private bool isInPlacementMode = false;
    private GameObject currentGhostObject;
    private ItemData currentPlaceableItem;
    private PlacementGhost ghostScript;
    private bool canPlace = false;
    #endregion
    #region Properties
    public Vector2 direction => _direction;
    public bool isRunning => _isRunning;
    public bool isRolling => _isRolling;
    public bool isAttacking => _isAttacking;
    public bool IsBusy() => _isRolling || isFishing || _isCutting || _isDigging || _isWatering || _isMining || _isAttacking;
    public bool IsDead() => _isDead;
    #endregion
    #region Unity Callbacks
    private void Awake() { rig = GetComponent<Rigidbody2D>(); playerAnimationController = GetComponent<PlayerAnim>(); spriteRenderer = GetComponent<SpriteRenderer>(); playerFishing = GetComponent<PlayerFishing>(); currentHealth = maxHealth; currentStamina = maxStamina; }
    private void Start() { initialSpeed = speed; if (spriteRenderer != null) originalSpriteColor = spriteRenderer.color; UseHotbarItem(currentHotbarIndex); }
    private void Update() { if (IsDead()) return; HandleInput(); if (isInPlacementMode) { HandlePlacementMode(); } if (Input.GetKeyDown(KeyCode.Tab)) { if (InventoryManager.instance != null) { InventoryManager.instance.ToggleInventory(!InventoryManager.instance.IsInventoryOpen()); } } if (isPaused || IsBusy()) { if (_isAttacking && canChainToNextCombo && Input.GetMouseButtonDown(0)) HandleAttackInput(); if (_isAttacking && canChainToNextCombo && (Time.time - lastChainInputTime > comboChainWindowDuration)) FinishAttackSequence(); else if (_isAttacking && !canChainToNextCombo && attackComboCount > 0 && (Time.time - lastChainInputTime > comboChainWindowDuration + 0.5f)) FinishAttackSequence(); return; } HandleRun(); HandleStaminaRegen(); HandleHotbarInput(); HandleActionInputs(); if (Input.GetKeyDown(useItemKey)) { PlayerItens.instance.ConsumeFirstAvailableHotbarItem(); } }
    private void FixedUpdate() { if (IsDead()) { rig.velocity = Vector2.zero; return; } if (IsBusy()) { if (_isRolling) { rig.velocity = rollDirection.normalized * rollSpeed; } else { rig.velocity = Vector2.zero; } } else { HandleMovement(); } }
    #endregion
    #region Lógica de Posicionamento
    private void HandlePlacementMode() { Vector3 placementPoint = transform.position + ((Vector3)lastMoveDirection.normalized * placementDistance); Vector3Int gridPosition = grid.WorldToCell(placementPoint); Vector3 ghostPosition = grid.GetCellCenterWorld(gridPosition); currentGhostObject.transform.position = ghostPosition; canPlace = !Physics2D.OverlapCircle(ghostPosition, 0.1f, placementObstacleLayerMask); ghostScript.SetValidity(canPlace); if (Input.GetKeyDown(secondaryActionKey) && canPlace) { PlaceItem(ghostPosition); } }
    private void EnterPlacementMode(ItemData itemToPlace) { ExitPlacementMode(); isInPlacementMode = true; currentPlaceableItem = itemToPlace; currentGhostObject = Instantiate(itemToPlace.placeablePrefab); ghostScript = currentGhostObject.AddComponent<PlacementGhost>(); foreach (var collider in currentGhostObject.GetComponents<Collider2D>()) { collider.enabled = false; } foreach (var monoBehaviour in currentGhostObject.GetComponents<MonoBehaviour>()) { if (!(monoBehaviour is Transform) && !(monoBehaviour is SpriteRenderer) && monoBehaviour != ghostScript) { monoBehaviour.enabled = false; } } }
    private void ExitPlacementMode() { if (isInPlacementMode) { isInPlacementMode = false; if (currentGhostObject != null) { Destroy(currentGhostObject); } currentGhostObject = null; currentPlaceableItem = null; ghostScript = null; } }
    private void PlaceItem(Vector3 position) { Instantiate(currentPlaceableItem.placeablePrefab, position, Quaternion.identity); PlayerItens.instance.RemoveQuantityFromSlot(ContainerType.Hotbar, currentHotbarIndex, 1); InventoryManager.instance.UpdateAllVisuals(); InventorySlot currentSlot = PlayerItens.instance.GetSlot(ContainerType.Hotbar, currentHotbarIndex); if (currentSlot == null || currentSlot.item == null || currentSlot.quantity == 0) { ExitPlacementMode(); } }
    #endregion
    private void UseHotbarItem(int slotIndex) { currentHotbarIndex = slotIndex; if (PlayerItens.instance == null) return; InventorySlot slot = PlayerItens.instance.GetSlot(ContainerType.Hotbar, slotIndex); if (HotbarController.instance != null) HotbarController.instance.UpdateSelection(slotIndex); if (slot == null || slot.item == null) { currentTool = ToolType.None; ExitPlacementMode(); return; } if (slot.item.itemType == ItemType.Placeable) { EnterPlacementMode(slot.item); currentTool = ToolType.None; } else { ExitPlacementMode(); switch (slot.item.itemType) { case ItemType.Ferramenta: currentTool = slot.item.associatedTool; break; case ItemType.Semente: currentTool = ToolType.None; break; default: currentTool = ToolType.None; break; } } }
    #region Métodos Antigos
    private void HandleInput() { _direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); if (_direction.sqrMagnitude > 0.01f) { lastMoveDirection = _direction.normalized; } }
    private void HandleMovement() { if (rig != null) rig.MovePosition(rig.position + _direction.normalized * (speed * speedMultiplier) * Time.fixedDeltaTime); }
    private void HandleActionInputs() { if (Input.GetMouseButtonDown(1)) TryStartRolling(); if (Input.GetKeyDown(secondaryActionKey) && !isInPlacementMode) { Vector3Int facingTile = grid.WorldToCell(playerActionPoint.position); if (FarmingManager.instance.CanHarvest(facingTile)) { FarmingManager.instance.Harvest(facingTile); } } if (Input.GetMouseButtonDown(0)) { InventorySlot currentSlot = PlayerItens.instance.GetSlot(ContainerType.Hotbar, currentHotbarIndex); if (currentSlot?.item?.itemType == ItemType.Semente) { Vector3Int facingTile = grid.WorldToCell(playerActionPoint.position); FarmingManager.instance.Plant(facingTile, currentSlot.item.cropData); } else if (currentTool == ToolType.Sword) { HandleAttackInput(); } else if (currentTool != ToolType.None) { HandleToolAction(); } } }
    public void OpenNextComboWindow() { if (_isAttacking && attackComboCount < 3) { canChainToNextCombo = true; lastChainInputTime = Time.time; isPaused = false; } else if (_isAttacking && attackComboCount >= 3) { FinishAttackSequence(); } }
    public void TakeDamage(float damageAmount) { if (isInvulnerable || IsDead()) return; currentHealth -= damageAmount; TakeDamageFeedback(); if (currentHealth <= 0) { currentHealth = 0; Die(); } }
    private void Die() { _isDead = true; isPaused = true; playerAnimationController.TriggerDeathAnimation(); GetComponent<Collider2D>().enabled = false; rig.velocity = Vector2.zero; rig.isKinematic = true; Invoke(nameof(ShowGameOverScreen), deathSequenceDelay); }
    private void ShowGameOverScreen() { UIManager uiManager = FindObjectOfType<UIManager>(); if (uiManager != null) { uiManager.ShowGameOverScreen(); } }
    private void HandleToolAction() { isPaused = true; switch (currentTool) { case ToolType.Axe: _isCutting = true; break; case ToolType.Shovel: _isDigging = true; break; case ToolType.WateringCan: _isWatering = true; break; case ToolType.Pickaxe: _isMining = true; break; case ToolType.FishingRod: if (playerFishing != null) playerFishing.StartFishingAction(); break; } if (playerAnimationController != null) { if (currentTool == ToolType.Pickaxe) playerAnimationController.TriggerMineAnimation(); else if (currentTool != ToolType.FishingRod) playerAnimationController.TriggerToolAnimation(currentTool); } else { OnActionFinished(); } }
    private void HandleStaminaRegen() { if (!_isRunning && !_isRolling) { if (currentStamina < maxStamina) { currentStamina += staminaRegenRate * Time.deltaTime; currentStamina = Mathf.Min(currentStamina, maxStamina); } } }
    public void ApplySlow() { activeSlowsCount++; if (activeSlowsCount == 1) speedMultiplier = slowEffectMultiplier; }
    public void RemoveSlow() { activeSlowsCount--; if (activeSlowsCount <= 0) { activeSlowsCount = 0; speedMultiplier = 1f; } }
    public void RestoreHealth(float amount) { if (amount <= 0 || IsDead()) return; currentHealth += amount; currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); }
    public void RestoreStamina(float amount) { if (amount <= 0) return; currentStamina += amount; currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina); }
    private void HandleRun() { bool wantsToRun = Input.GetKey(KeyCode.LeftShift); if (wantsToRun && _direction.sqrMagnitude > 0 && currentStamina > 0) { speed = runSpeed; _isRunning = true; currentStamina -= runStaminaCostPerSecond * Time.deltaTime; } else { speed = initialSpeed; _isRunning = false; } }
    private void TryStartRolling() { if (currentStamina >= rollStaminaCost) { currentStamina -= rollStaminaCost; rollDirection = (_direction.sqrMagnitude > 0.01f) ? _direction : lastMoveDirection; if (rollDirection.sqrMagnitude > 0.01f) { ExitPlacementMode(); isInvulnerable = true; isPaused = true; _isRolling = true; playerAnimationController.TriggerRollAnimation(rollDirection); } } }
    private void HandleHotbarInput() { if (PlayerItens.instance == null) return; float scroll = Input.GetAxis("Mouse ScrollWheel"); if (scroll != 0f) { currentHotbarIndex += scroll < 0f ? 1 : -1; if (currentHotbarIndex >= PlayerItens.instance.hotbarSize) currentHotbarIndex = 0; if (currentHotbarIndex < 0) currentHotbarIndex = PlayerItens.instance.hotbarSize - 1; UseHotbarItem(currentHotbarIndex); return; } for (int i = 0; i < PlayerItens.instance.hotbarSize; i++) { if (Input.GetKeyDown(KeyCode.Alpha1 + i)) { currentHotbarIndex = i; UseHotbarItem(currentHotbarIndex); return; } } }
    private void OnActionFinished() { isPaused = false; _isRolling = false; _isCutting = false; _isDigging = false; _isWatering = false; _isMining = false; isFishing = false; if (rig != null) rig.velocity = Vector2.zero; if (_isAttacking) { _isAttacking = false; attackComboCount = 0; canChainToNextCombo = false; if (playerAnimationController != null) playerAnimationController.ResetAttackAnimationParams(); } }
    private void HandleAttackInput() { if (!_isAttacking) { isPaused = true; _isAttacking = true; attackComboCount = 1; canChainToNextCombo = false; playerAnimationController.TriggerAttackAnimation(attackComboCount); lastChainInputTime = Time.time; } else if (canChainToNextCombo && attackComboCount < 3) { isPaused = true; attackComboCount++; canChainToNextCombo = false; playerAnimationController.TriggerAttackAnimation(attackComboCount); lastChainInputTime = Time.time; } }
    public void PerformToolActionCheck() { if (playerActionPoint == null || grid == null) return; Vector3Int gridPosition = grid.WorldToCell(playerActionPoint.position); switch (currentTool) { case ToolType.Shovel: FarmingManager.instance.Dig(gridPosition); break; case ToolType.WateringCan: FarmingManager.instance.Water(gridPosition); break; case ToolType.Axe: case ToolType.Pickaxe: Collider2D[] hitObjects = Physics2D.OverlapCircleAll(playerActionPoint.position, playerActionRadius, interactableLayer); foreach (Collider2D hit in hitObjects) { hit.GetComponent<MineableResource>()?.OnHit(); } break; case ToolType.FishingRod: if (playerFishing != null) playerFishing.SpawnFishIfCaught(); break; } }
    public void PerformSwordAttack() { if (playerActionPoint == null) return; Collider2D[] hitObjects = Physics2D.OverlapCircleAll(playerActionPoint.position, playerActionRadius, interactableLayer); foreach (Collider2D hit in hitObjects) { hit.GetComponent<IDamageable>()?.TakeDamage(playerAttackDamage); } }
    public void FinishRolling() { isInvulnerable = false; OnActionFinished(); }
    public void FinishCurrentAction() => OnActionFinished();
    public void FinishAttackSequence() => OnActionFinished();
    public void TakeDamageFeedback() { if (isInvulnerable) return; isInvulnerable = true; playerAnimationController.TriggerTakeHitAnimation(); if (damageFlashCoroutine != null) StopCoroutine(damageFlashCoroutine); damageFlashCoroutine = StartCoroutine(FlashRedCoroutine()); if (invulnerabilityCoroutine != null) StopCoroutine(invulnerabilityCoroutine); invulnerabilityCoroutine = StartCoroutine(InvulnerabilityWindowCoroutine()); }
    private IEnumerator FlashRedCoroutine() { spriteRenderer.color = Color.red; yield return new WaitForSeconds(0.1f); spriteRenderer.color = originalSpriteColor; }
    private IEnumerator InvulnerabilityWindowCoroutine() { yield return new WaitForSeconds(invulnerabilityDuration); isInvulnerable = false; }
    private void OnDrawGizmosSelected() { if (playerActionPoint != null) { Gizmos.color = Color.blue; Gizmos.DrawWireSphere(playerActionPoint.position, playerActionRadius); } }
    #endregion
}