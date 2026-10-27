using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseState : IEnemyState
{
    private readonly InfantryAI ai;
    private readonly TargetDetector detector;
    private readonly EnemyAim aim;
    private readonly EntityMotor motor;
    private readonly EnemyData data;

    private float strafeDirection = 1f;
    private float strafeTimer;
    private bool isStrafing = false;
    private const float SPACING_BUFFER = 0.4f;

    private List<Vector2> currentPath;
    private int currentWaypointIndex = 0;
    private float pathUpdateTimer;
    private const float PATH_RECALC_INTERVAL = 0.25f;
    private const float WAYPOINT_REACHED_RADIUS = 0.3f;

    private float debugTimer;

    public EnemyChaseState(InfantryAI ai, TargetDetector detector, EnemyAim aim, EntityMotor motor, EnemyData data)
    {
        this.ai = ai;
        this.detector = detector;
        this.aim = aim;
        this.motor = motor;
        this.data = data;
    }

    public void Enter() 
    {
        if (Random.value > 0.5f) 
        {
            strafeDirection = 1f;
        }
        else
        {
            strafeDirection = -1f;
        }

        strafeTimer = Random.Range(1f, 2.5f);
        isStrafing = false;

        currentPath = null;
        currentWaypointIndex = 0;
        pathUpdateTimer = 0f;
    }

    public void UpdateState()
    {
        Transform target = detector.CurrentTarget;
        if (!detector.HasTarget || data == null)
        {
            ai.ChangeState(ai.IdleState);
            return;
        }

        Vector2 directionToTarget = detector.DirectionToTarget;
        float distanceToTarget = detector.DistanceToTarget;

        if (aim != null)
        {
            aim.UpdateAim(directionToTarget);
        }

        //Debug.Log($"Distance: {distanceToTarget} | CanAttack: {ai.CanAttack} | MeleeRange: {data.meleeAttackRange}");

        if (ai.CanAttack)
        {
            if (distanceToTarget <= data.meleeAttackRange && data.meleeAttackData.Length > 0)
            {
                // pick random melee attack from melee attack list
                int randomIndex = Random.Range(0, data.meleeAttackData.Length);
                ai.SelectAttack(data.meleeAttackData[randomIndex]);
                ai.ChangeState(ai.AttackState);
                return;
            }
            else if (distanceToTarget <= data.rangedAttackRange && data.rangeAttackData != null)
            {
                ai.SelectAttack(data.rangeAttackData);
                ai.ChangeState(ai.AttackState);
                return;
            }
        }

        float innerThreshold = data.meleeAttackRange;
        float outerThreshold = data.meleeAttackRange + SPACING_BUFFER;

        if (!isStrafing && distanceToTarget <= innerThreshold)
        {
            isStrafing = true;
        }
        else if (isStrafing && distanceToTarget > outerThreshold) 
        {
            isStrafing = false;
        }

        if (motor == null)
        {
            return;
        }

        if (isStrafing)
        {
            strafeTimer -= Time.deltaTime;
            if (strafeTimer <= 0f)
            {
                strafeDirection *= 1f;
                strafeTimer = Random.Range(1f, 2.5f);
            }

            Vector2 perpendicular = new Vector2(-directionToTarget.y, directionToTarget.x) * strafeDirection;
            motor.Move(perpendicular.normalized, data.chaseSpeed * 0.5f);
        }
        else
        {
            pathUpdateTimer -= Time.deltaTime;
            if (pathUpdateTimer <= 0f)
            {
                pathUpdateTimer = PATH_RECALC_INTERVAL;
                if (AStarPathFinder.Instance != null)
                {
                    currentPath = AStarPathFinder.Instance.FindPath(ai.transform.position, target.position);
                    currentWaypointIndex = 0;
                }
            }

            if (currentPath != null && currentWaypointIndex < currentPath.Count)
            {
                Vector2 targetWaypoint = currentPath[currentWaypointIndex];
                Vector2 moveDirection = (targetWaypoint - (Vector2)ai.transform.position).normalized;
            
                if (Vector2.Distance(ai.transform.position, targetWaypoint) <= WAYPOINT_REACHED_RADIUS)
                {
                    currentWaypointIndex++;
                }

                motor.Move(moveDirection, data.chaseSpeed);
            }
            else
            {
                motor.Move(directionToTarget, data.chaseSpeed);
            }
        }
    }

    public void Exit() 
    {
        if (motor != null)
        {
            motor.Move(Vector2.zero, 0f);
        }
    }
}
