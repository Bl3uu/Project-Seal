using UnityEngine;
using UnityEngine.Rendering;

public enum AttackType
{
    Melee,
    Flintlock
}

[CreateAssetMenu(fileName = "NewAttackData", menuName = "Combat/Attack Data")]
public class AttackData : ScriptableObject
{
    [Header("Attack Classification")]
    public AttackType attackType = AttackType.Melee;

    [Tooltip("Distance forward from origin to cast the hitbox")]    
    public float attackDistance = 1.2f;

    [Header("Payload Stats")]
    public float baseDamage = 25f;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    [Header("Movement & Animation")]
    public float lungeForce = 3f;
    public string animationTriggerName;
    public GameObject vfxPrefab;

    [Header("Dynamic Frame Timings")]
    [Tooltip("Duration the stays in the active hitframes before entering recovery.")]
    public float activeHitDuration = 0.1f;
    [Tooltip("Duration of recovery phase where cancels or sequence follow-ups are permitted")]
    public float recoveryDuration = 0.25f;

    [Header("Audio")]
    public AudioClip attackSFX;
    public AudioClip hitImpactSFX;

    [Header("Sequence Branching (Follow-ups)")]
    public AttackData nextMeleeFollowUp;
    public AttackData nextFlintlockFollowUp;
}
