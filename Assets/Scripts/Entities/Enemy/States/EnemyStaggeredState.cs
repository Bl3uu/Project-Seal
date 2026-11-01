using UnityEngine;

public class EnemyStaggeredState : IEnemyState
{
    private readonly Enemy enemy;
    private readonly EntityMotor motor;
    private float staggerTimer;
    private float staggerDuration;

    public EnemyStaggeredState(Enemy enemy, EntityMotor motor, float duration = 1.5f)
    {
        this.enemy = enemy;
        this.motor = motor;
        this.staggerDuration = duration;
    }

    public void Enter()
    {
        staggerTimer = 0f;
        if (motor != null)
        {
            motor.Move(Vector2.zero, 0f);
        }
    }

    public void UpdateState() 
    { 
        staggerTimer += Time.deltaTime;

        if (staggerTimer >= staggerDuration) 
        {
            enemy.ChangeState(null);
        }

    }

    public void FixedUpdateState() { }
    
    public void Exit() { }
}
