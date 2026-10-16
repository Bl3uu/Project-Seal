using Unity.VisualScripting;
using UnityEngine;

public class EnemyStaggeredState : IEnemyState
{
    private readonly EntityMotor motor;

    public EnemyStaggeredState(EntityMotor motor)
    {
        this.motor = motor;
    }

    public void Enter()
    {
        if (motor != null)
        {
            motor.Move(Vector2.zero, 0f);
        }
    }

    public void UpdateState() { }
    
    public void Exit() { }
}
