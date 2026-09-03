using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ComboInputType
{
    Melee,
    Flintlock,
    Skill
}

public struct ComboStepData
{
    public int StepIndex; // 1, 2, or 3
    public ComboInputType InputType;
    public Vector2 AimDirection;
    public Vector2 AimWorldPosition;
}

public class PlayerAttack : MonoBehaviour
{
    [Header("Combo Configuration")]
    [Tooltip("Maximum allowed time (seconds) between inputs before the combo resets to Step 1.")]
    [SerializeField] private float comboResetWindow = 1.0f;

    [Tooltip("Global attack recovery duration before starting another step.")]
    [SerializeField] private float attackRecoveryTime = 0.25f;

    [Header("References")]
    [SerializeField] private Transform attackOrigin;

    private IAimProvider aimProvider;
    private HealthComponent healthComponent;

    // Combo Pipeline State
    private int currentComboStep = 0; // 0 = Idle, 1 = Step 1, 2 = Step 2, 3 = Step 3
    private bool isAttacking = false;
    private float comboResetTimer = 0f;

    public int CurrentComboStep => currentComboStep;
    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        aimProvider = GetComponent<IAimProvider>();
        healthComponent = GetComponent<HealthComponent>();

        if (attackOrigin == null)
        {
            attackOrigin = transform;
        }
    }

    private void Update()
    {
        // Keep decay timer running frame-by-frame
        HandleComboDecay();
    }

    #region Input System Callbakcs

    // Bound to Left Mouse Button (LMB) in Player Input
    public void OnMeleeInput(InputValue value)
    {
        if (value.isPressed)
        {
            ExecuteComboInput(ComboInputType.Melee);
        }
    }

    // Bound to Right Mouse Button (RMB) in Player Input
    public void OnFlintlockInput(InputValue value)
    {
        if (value.isPressed)
        {
            ExecuteComboInput(ComboInputType.Flintlock);
        }
    }

    // Bound to Q, E, C, or Tab in Player Input
    public void OnSkillInput(InputValue value)
    {
        if (value.isPressed)
        {
            ExecuteComboInput(ComboInputType.Skill);
        }
    }

    public void OnFreeFireFlintlockInput(InputValue value)
    {
        if (value.isPressed)
        {
            ExecuteFreeFireFlintlock();
        }
    }

    #endregion

    private void ExecuteFreeFireFlintlock()
    {
        Debug.Log("[PlayerAttack] FREE-FIRE FLINTLOCK executed off-GCD (Combo sequence unaffected).");
        // Delegates directly to FlintlockCarousel without touching currentComboStep
    }

    // Evaluates decay timer. Resets combo sequence back to Step 1 if player waits too long.
    private void HandleComboDecay()
    {
        if (currentComboStep > 0 && !isAttacking)
        {
            comboResetTimer -= Time.deltaTime;
            if (comboResetTimer <= 0f)
            {
                ResetCombo();
            }
        }
    }

    // Entry point for triggering any combat input (Melee / Flintlock / Skill)
    public void ExecuteComboInput(ComboInputType inputType)
    {
        if (isAttacking)
        {
            return; // Busy in active execution/recovery frames
        }

        // Advance step (1, 2, or 3)
        currentComboStep++;
        if (currentComboStep > 3)
        {
            currentComboStep = 1;
        }

        // Capture snapshot from IAimProvider
        Vector2 aimDir;
        Vector2 aimPos;

        if (aimProvider != null)
        {
            aimDir = aimProvider.AimDirection;
        }
        else
        {
            aimDir = Vector2.down;
        }

        if (aimProvider != null)
        {
            aimPos = aimProvider.AimWorldPosition;
        }
        else
        {
            aimPos = (Vector2)transform.position;
        }

        ComboStepData stepData = new ComboStepData
        {
            StepIndex = currentComboStep,
            InputType = inputType,
            AimDirection = aimDir,
            AimWorldPosition = aimPos
        };

        StartCoroutine(PerformAttackRoutine(stepData));
    }

    private IEnumerator PerformAttackRoutine(ComboStepData stepData)
    {
        isAttacking = true;

        // Route to execution logic based on input type
        switch (stepData.InputType)
        {
            case ComboInputType.Melee:
                ExecuteMeleeAttack(stepData);
                break;
            case ComboInputType.Flintlock:
                ExecuteFlintlockAttack(stepData);
                break;
            case ComboInputType.Skill:
                ExecuteSkillAttack(stepData);
                break;
        }

        // Active recovery delay
        yield return new WaitForSeconds(attackRecoveryTime);

        isAttacking = false;
        comboResetTimer = comboResetWindow;

        // Auto-reset if step 3 just completed
        if (currentComboStep >= 3)
        {
            ResetCombo();
        }
    }

    private void ExecuteMeleeAttack(ComboStepData stepData)
    {
        Debug.Log($"[PlayerAttack] Executing MELEE - Step {stepData.StepIndex} | Aim Dir: {stepData.AimDirection}");

        // Example Boxcast check along aim vector
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            attackOrigin.position,
            new Vector2(1.5f, 1.5f),
            0f,
            stepData.AimDirection,
            1.2f
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    DamageData damagePayload = new DamageData
                    {
                        Amount = 25f * stepData.StepIndex, // Escalating step damage
                        HitDirection = stepData.AimDirection,
                        Source = gameObject
                    };

                    damageable.TakeDamage(damagePayload);
                }
            }
        }
    }

    private void ExecuteFlintlockAttack(ComboStepData stepData)
    {
        Debug.Log($"[PlayerAttack] Executing FLINTLOCK CAROUSEL - Step {stepData.StepIndex}");
        // Delegates to FlintlockCarousel execution logic here
    }

    private void ExecuteSkillAttack(ComboStepData stepData)
    {
        Debug.Log($"[PlayerAttack] Executing SKILL - Step {stepData.StepIndex}");
        // Delegates to active Skill execution logic here
    }

    public void ResetCombo()
    {
        currentComboStep = 0;
        comboResetTimer = 0f;
        isAttacking = false;
        Debug.Log("[PlayerAttack] Combo Sequence Reset.");
    }
}
