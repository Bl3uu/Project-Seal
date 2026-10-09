using UnityEngine;

public static class AnimatorExtensions
{
    public static float GetAnimationLengthOrDefault(this Component component, float fallbackDuration = 0.3f)
    {
        if (component.TryGetComponent<Animator>(out var animator))
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.length > 0f)
            {
                return stateInfo.length;
            }
        }

        return fallbackDuration;
    }
}
