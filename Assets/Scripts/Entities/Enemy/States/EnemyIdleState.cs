using UnityEngine;

public class EnemyIdleState : IEnemyState
{
    private readonly InfantryAI ai;
    private readonly TargetDetector detector;

    public EnemyIdleState(InfantryAI ai, TargetDetector detector)
    {
        this.ai = ai;
        this.detector = detector;
    }

    public void Enter() { }

    public void UpdateState()
    {
        float sightRadius;
        if (ai.Data != null)
        {
            sightRadius = ai.Data.sightRadius;
        }
        else
        {
            sightRadius = 8f;
        }

        if (detector != null && detector.DetectTargetInRadius(sightRadius))
        {
            ai.ChangeState(ai.ChaseState);
        }
    }

    public void Exit() { }
}
