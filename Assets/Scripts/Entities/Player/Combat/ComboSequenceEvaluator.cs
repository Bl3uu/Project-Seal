using UnityEngine;

public class ComboSequenceEvaluator
{
    private AttackData entryMeleeAttackData;
    private AttackData entryFlintlockAttackData;

    public ComboSequenceEvaluator(AttackData entryMelee, AttackData entryFlintlock)
    {
        this.entryMeleeAttackData = entryMelee;
        this.entryFlintlockAttackData = entryFlintlock;
    }

    public AttackData EvaluateNextNode(PlayerActionType input, AttackData currentAttackData, int currentStep)
    {
        if (currentAttackData == null || currentStep == 0)
        {
            if (input == PlayerActionType.Melee)
            {
                return entryMeleeAttackData;
            }
            if (input == PlayerActionType.Flintlock)
            {
                return entryFlintlockAttackData;
            }

            return null;
        }

        AttackData targetNode = null;

        if (input == PlayerActionType.Melee)
        {
            targetNode = currentAttackData.nextMeleeFollowUp;
        }
        else if (input == PlayerActionType.Flintlock)
        {
            targetNode = currentAttackData.nextFlintlockFollowUp;
        }

        if (targetNode != null && targetNode.attackType != input)
        {
            Debug.LogError($"[ComboSequenceEvaluator] Mismatched AttackType on node '{targetNode.name}'" +
                           $"Expected {input}, but node is typed as {targetNode.attackType}. Blocking sequence step.");
            return null;
        }

        return targetNode;
    }
}
