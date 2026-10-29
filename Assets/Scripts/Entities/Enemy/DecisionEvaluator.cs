using System.Collections.Generic;
using UnityEngine;

public enum MovementIntent
{
    Chase,
    Backpedal,
    HoldGround,
    Flank,
    Patrol,
}

public class DecisionEvaluator : MonoBehaviour 
{
    [Header("Reference")]
    [SerializeField] private Enemy enemy;
    [SerializeField] private TargetDetector targetDetector;
    [SerializeField] private AmmoTracker ammoTracker;

    private EnemyData Data;

    private void Awake()
    {
        if (enemy != null)
        {
            Data = enemy.Data;
        }
        else
        {
            Data = null;
        }
    }

    public AttackData DetermineAttack(float distanceToTarget)
    {
        if (Data == null || targetDetector == null)
        {
            return null;
        }

        if (!targetDetector.HasLineOfSight())
        {
            return null;
        }

        List<(AttackData attack, int weight)> candidates = new List<(AttackData, int)>();

        if (Data.meleeAttackData != null)
        {
            foreach (var attack in Data.meleeAttackData)
            {
                if (attack == null)
                {
                    continue;
                }

                if (distanceToTarget >= attack.minExecutionRange && distanceToTarget <= attack.maxExecutionRange)
                {
                    candidates.Add((attack, attack.baseWeight));
                }
            }
        }

        bool hasAmmo = ammoTracker == null || ammoTracker.HasBullet;

        if (Data.rangeAttackData != null && hasAmmo)
        {
            var ranged = Data.rangeAttackData;
            if (distanceToTarget >= ranged.minExecutionRange && distanceToTarget <= ranged.maxExecutionRange)
            {
                candidates.Add((ranged, ranged.baseWeight));
            }
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        int totalWeight = 0;
        foreach (var c in candidates)
        {
            totalWeight += c.weight;
        }

        int roll = Random.Range(0, totalWeight);
        int currentSum = 0;

        foreach (var c in candidates)
        {
            currentSum += c.weight;
            if (roll < currentSum)
            {
                return c.attack;
            }
        }

        return null;
    }

    public MovementIntent EvaluateMovement(float distanceToTarget)
    {
        if (ammoTracker != null && ammoTracker.IsReloading)
        {
            if (distanceToTarget < 0.3f)
            {
                return MovementIntent.Backpedal;
            }
            else
            {
                return MovementIntent.HoldGround;
            }
        }

        if (Data != null && distanceToTarget <= Data.rangedAttackRange && distanceToTarget > Data.meleeAttackRange)
        {
            if (Random.value > 0.4f)
            {
                return MovementIntent.Flank;
            }
            else
            {
                return MovementIntent.Chase;
            }
        }

        return MovementIntent.Chase;
    }

    private float GetAggresionMultiplier()
    {
        if (enemy == null)
        {
            return 1f;
        }

        if (enemy.HealthPercentage <= 0.3f)
        {
            return 0.5f;
        }
        else
        {
            return 1f;
        }
    }
}
