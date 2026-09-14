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

    public void StartStep()
    {
        CurrentStep++;
        CurrentState = ComboState.Active;
        Debug.Log($"[ComboStateMachine] Step {CurrentStep} Active");
    }

    public void EnterRecovery()
    {
        if (CurrentState == ComboState.Active)
        {
            CurrentState = ComboState.Recovery;
            Debug.Log($"[ComboStateMachine] State -> Recovery");
        }
    }

    public void Reset()
    {
        CurrentStep = 0;
        CurrentState = ComboState.Idle;
        Debug.Log("[ComboStateMachine] State -> Idle");
    }

    public void CompleteAttack()
    {
        CurrentState = ComboState.Idle;
        Debug.Log("[ComboStateMachine Step Complete -> Waiting for input window]");
    }
}
