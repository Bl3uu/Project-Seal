using System.Collections.Generic;
using UnityEngine;

public class EntityAnimator2D : MonoBehaviour, IAnimationController
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    private EntityMotor motor;
    private PlayerAim playerAim;
    private PlayerAttack playerAttack;
    private PlayerDash playerDash;

    [Header("Animations")]
    [SerializeField] private CharacterAnimationData animationDatabase;

    [Header("Settings")]
    [Tooltip("Dynamicamically toggle: ")]
    [SerializeField] private bool faceAimDirection = false;
    [Tooltip("Uncheck this for asymmetrical character designs so sprite graphics arent mirrored horizontally.")]
    [SerializeField] private bool useSpriteFlipping = false;

    private Dictionary<string, SpriteAnimationClip> clipDictionary = new Dictionary<string, SpriteAnimationClip>();
    private SpriteAnimationClip currentClip;
    private int currentFrameIndex = 0;
    private float timer = 0f;
    private bool isPlaying = false;

    private Vector2 cachedMoveDir;
    private Vector2 cachedAimDir;
    private string lastFacingDirection = "_Right";

    private void Awake()
    {
        motor = GetComponent<EntityMotor>();
        playerAim = GetComponent<PlayerAim>();
        playerAttack = GetComponent<PlayerAttack>();
        playerDash = GetComponent<PlayerDash>();

        if (animationDatabase != null)
        {
            foreach (var clip in animationDatabase.clips)
            {
                if (!clipDictionary.ContainsKey(clip.clipName))
                {
                    clipDictionary.Add(clip.clipName, clip);
                }
            }
        }
        else
        {
            Debug.LogError($"Animation Database is missing on {gameObject.name}!", this);
        }
    }

    private void Start()
    {
        if (useSpriteFlipping)
        {
            Play("Idle");
        }
        else
        {
            Play("Idle_Right");
        }
    }

    private void Update()
    {
        if (motor != null)
        {
            bool isAttacking = playerAttack != null && playerAttack.IsAttacking;
            bool isDashing = playerDash != null && playerDash.IsDashing;

            faceAimDirection = isAttacking;

            cachedMoveDir = motor.MoveDirection;

            if (playerAim != null)
            {
                cachedAimDir = playerAim.AimDirection;
            }

            if (!isAttacking && !isDashing)
            {
                UpdateLocomotion(cachedMoveDir, cachedAimDir);
            }
        }

        UpdateAnimationPlayback();
    }

    private void UpdateAnimationPlayback()
    {
        if (!isPlaying || currentClip == null || currentClip.frames.Length == 0)
        {
            return;
        }

        timer += Time.deltaTime;
        float frameDuration = 1f / currentClip.frameRate;

        if (timer >= frameDuration)
        {
            timer -= frameDuration;
            currentFrameIndex++;

            if (currentFrameIndex >= currentClip.frames.Length)
            {
                if (currentClip.loop)
                {
                    currentFrameIndex = 0;
                }
                else
                {
                    currentFrameIndex = currentClip.frames.Length - 1;
                    isPlaying = false;
                }
            }

            if (spriteRenderer != null && currentClip.frames[currentFrameIndex] != null)
            {
                spriteRenderer.sprite = currentClip.frames[currentFrameIndex];
            }
        }
    }

    public void Play(string clipName)
    {
        if (clipDictionary.TryGetValue(clipName, out SpriteAnimationClip clip))
        {
            if (currentClip == clip && isPlaying)
            {
                return;
            }

            currentClip = clip;
            currentFrameIndex = 0;
            timer = 0f;
            isPlaying = true;

            Debug.Log($"{gameObject.name} Switched Animation -> {clipName}");

            if (spriteRenderer != null && currentClip.frames.Length > 0)
            {
                spriteRenderer.sprite = currentClip.frames[0];
            }
        }
    }

    public void UpdateLocomotion(Vector2 movementInput, Vector2 lookDirection)
    {
        Vector2 targetDir;

        if (faceAimDirection && lookDirection != Vector2.zero)
        {
            targetDir = lookDirection;
        }
        else
        {
            targetDir = movementInput;
        }

        ApplySpriteFlipping(targetDir);

        string baseState;

        if (movementInput.magnitude > 0.1f)
        {
            baseState = "Run";
        }
        else
        {
            baseState = "Idle";
        }

        string targetClip = baseState + GetDirectionSuffix(targetDir);
        Play(baseState + GetDirectionSuffix(targetDir));
    }

    public void PlayAttack(int comboStep, float aimAngle)
    {
        Play("Attack" + GetDirectionSuffix(cachedAimDir));
    }

    public void PlayDash(Vector2 dashDirection)
    {
        ApplySpriteFlipping(dashDirection);
        Play("Dash" + GetDirectionSuffix(dashDirection));
    }

    public void PlayHurt()
    {
        Play("Hurt" + GetDirectionSuffix(Vector2.zero));
    }

    public void PlayDeath()
    {
        Play("Death" + GetDirectionSuffix(Vector2.zero));
        isPlaying = false;
    }

    private string GetDirectionSuffix(Vector2 direction)
    {
        if (useSpriteFlipping)
        {
            return "";
        }

        if (direction.x != 0)
        {
            if (direction.x < 0)
            {
                lastFacingDirection = "_Left";
            }
            else
            {
                lastFacingDirection = "_Right";
            }
        }

        return lastFacingDirection;
    }

    private void ApplySpriteFlipping(Vector2 direction)
    {
        if (useSpriteFlipping && direction.x != 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    public float GetCurrentClipDuration()
    {
        if (currentClip == null || currentClip.frameRate <= 0f)
        {
            return 0.3f;
        }
        return currentClip.frames.Length / currentClip.frameRate;
    }
}
