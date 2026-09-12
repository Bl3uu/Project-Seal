using UnityEngine;

public enum ComboState
{
    Idle,
    Active,
    Recovery
}
public class ComboStateMachine
{
    public ComboState CurrentState { get; private set; } = ComboState.Idle;
    public int CurrentStep { get; private set; } = 0;

    public bool CanCancel => CurrentState == ComboState.Recovery;
    public bool IsAttacking => CurrentState == ComboState.Active || CurrentState == ComboState.Recovery;

    public void StartStep(int maxSteps = 3)
    {
        CurrentStep++;
        if (CurrentStep > maxSteps)
        {
            CurrentStep = 1;
        }
        CurrentState = ComboState.Active;
        Debug.Log("[ComboStateMachine] Combo State is Active");
    }

    public void EnterRecovery()
    {
        if (CurrentState == ComboState.Active)
        {
            CurrentState = ComboState.Recovery;
        }
        Debug.Log("[ComboStateMachine] Combo State is Recovering");
    }

    public void Reset()
    {
        CurrentStep = 0;
        CurrentState = ComboState.Idle;
        Debug.Log("[ComboStateMachine] Combo State is Idle");
    }

    public void CompleteAttack()
    {
        CurrentState = ComboState.Idle;
    }
}
