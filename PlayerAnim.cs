using UnityEngine;

public enum PlayerAnimState { Idle = 0, WalkSide = 1, Cutting = 3, Dig = 4, Watering = 5, WalkUp = 6, WalkDown = 7, Dig_Up = 8, Dig_Down = 9, Water_Up = 10, Water_Down = 11, Hammering = 12 }

public class PlayerAnim : MonoBehaviour
{
    private Player player;
    private Animator anim;
    private bool isFishingAnimationRunning;

    // Hashes dos parâmetros do Animator
    private static readonly int TransitionHash = Animator.StringToHash("Transition");
    private static readonly int IsRollTriggerHash = Animator.StringToHash("isRoll");
    private static readonly int IsCastingHash = Animator.StringToHash("isCasting");
    private static readonly int RollDirectionHash = Animator.StringToHash("RollDirection");
    private static readonly int DoAttackTriggerHash = Animator.StringToHash("DoAttack");
    private static readonly int DoMineTriggerHash = Animator.StringToHash("DoMine");
    private static readonly int TakeHitTriggerHash = Animator.StringToHash("TakeHit");
    private static readonly int AttackComboStepHash = Animator.StringToHash("AttackComboStep");
    private static readonly int AttackAnimDirectionHash = Animator.StringToHash("AttackAnimDirection");
    private static readonly int AnimationSpeedMultiplierHash = Animator.StringToHash("AnimationSpeedMultiplier");
    private static readonly int IsDeadTriggerHash = Animator.StringToHash("IsDead"); // << NOVO

    [Header("Animation Speed Settings")]
    [SerializeField] private float walkAnimSpeed = 1.0f;
    [SerializeField] private float runAnimSpeedMultiplier = 1.5f;

    void Start()
    {
        player = GetComponent<Player>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null || anim == null || player.IsDead()) return; // << Adicionado player.IsDead() para parar tudo ao morrer

        if (player.isFishing)
        {
            if (!isFishingAnimationRunning)
            {
                OnCastingStarted();
            }
            return;
        }
        else
        {
            if (isFishingAnimationRunning)
            {
                OnCastingEnded();
            }
        }

        if (player.IsBusy() && !player.isFishing)
        {
            return;
        }

        if (player.isRunning && player.direction.sqrMagnitude > 0.01f)
        {
            anim.SetFloat(AnimationSpeedMultiplierHash, runAnimSpeedMultiplier);
        }
        else
        {
            anim.SetFloat(AnimationSpeedMultiplierHash, walkAnimSpeed);
        }

        if (player.direction.sqrMagnitude > 0.01f)
        {
            HandleMovementOnly();
        }
        else
        {
            anim.SetInteger(TransitionHash, (int)PlayerAnimState.Idle);
        }
    }

    private int GetDirectionalIntValue(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            transform.localScale = new Vector3(Mathf.Sign(dir.x), 1f, 1f);
            return 0;
        }
        else
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
            if (dir.y > 0) return 1;
            else return 2;
        }
    }

    private void HandleMovementOnly()
    {
        int animDirectionParamValue = GetDirectionalIntValue(player.direction);
        if (player.direction.sqrMagnitude > 0.01f)
        {
            player.lastMoveDirection = player.direction.normalized;
        }

        if (animDirectionParamValue == 0) anim.SetInteger(TransitionHash, (int)PlayerAnimState.WalkSide);
        else if (animDirectionParamValue == 1) anim.SetInteger(TransitionHash, (int)PlayerAnimState.WalkUp);
        else anim.SetInteger(TransitionHash, (int)PlayerAnimState.WalkDown);
    }

    public void TriggerToolAnimation(ToolType tool)
    {
        if (anim == null) return;
        Vector2 facingDir = player.lastMoveDirection;
        anim.SetInteger(AttackAnimDirectionHash, GetDirectionalIntValue(facingDir));

        int toolState;
        switch (tool)
        {
            case ToolType.Axe: toolState = (int)PlayerAnimState.Cutting; break;
            case ToolType.Shovel: toolState = (int)PlayerAnimState.Dig; break;
            case ToolType.WateringCan: toolState = (int)PlayerAnimState.Watering; break;
            default: toolState = (int)PlayerAnimState.Idle; break;
        }
        anim.SetInteger(TransitionHash, toolState);
    }

    public void TriggerMineAnimation()
    {
        if (anim == null) return;
        Vector2 facingDir = player.lastMoveDirection;
        anim.SetInteger(AttackAnimDirectionHash, GetDirectionalIntValue(facingDir));
        anim.SetTrigger(DoMineTriggerHash);
    }

    // << NOVO MÉTODO >>
    public void TriggerDeathAnimation()
    {
        if (anim != null)
        {
            anim.SetTrigger(IsDeadTriggerHash);
        }
    }

    public void TriggerRollAnimation(Vector2 rollDirection) { if (anim == null) return; anim.SetInteger(RollDirectionHash, GetDirectionalIntValue(rollDirection)); anim.SetTrigger(IsRollTriggerHash); }
    public void TriggerAttackAnimation(int comboStep) { if (anim == null) return; Vector2 facingDir = player.lastMoveDirection; anim.SetInteger(AttackAnimDirectionHash, GetDirectionalIntValue(facingDir)); anim.SetInteger(AttackComboStepHash, comboStep); anim.SetTrigger(DoAttackTriggerHash); }
    public void ResetAttackAnimationParams() { if (anim == null) return; anim.SetInteger(AttackComboStepHash, 0); anim.ResetTrigger(DoAttackTriggerHash); }
    public void TriggerTakeHitAnimation() { if (anim != null) anim.SetTrigger(TakeHitTriggerHash); }
    public void OnCastingStarted() { if (anim != null) { isFishingAnimationRunning = true; anim.SetTrigger(IsCastingHash); } }
    public void OnCastingEnded() { isFishingAnimationRunning = false; }
}