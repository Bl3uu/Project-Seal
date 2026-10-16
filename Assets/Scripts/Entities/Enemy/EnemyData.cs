using UnityEngine;

[CreateAssetMenu(fileName = "SO_EnemyData_New", menuName = "ProjectSeal/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Base Identity")]
    public string enemyName = "Enemy Name";

    [Header("Health & Defence")]
    public float maxHealth = 100f;
    public float maxPoise = 50f;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float chaseSpeed = 4.5f;

    [Header("Perceptioon & Engagement")]
    public float sightRadius = 8f;
    public float meleeAttackRange = 1.5f;
    public float rangedAttackRange = 7f;

    [Header("Attacks")]
    public AttackData[] meleeAttackData;
    public AttackData rangeAttackData;

    [Header("Cooldowns")]
    public float globalAttackCooldown = 1.5f;
}
