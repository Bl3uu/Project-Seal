using UnityEngine;

public interface IAnimationController
{
    void UpdateLocomotion(Vector2 movementInput, Vector2 lookDirection);
    void PlayAttack(int comboStep, float aimAngle);
    void PlayDash(Vector2 dashDirection);
    void PlayHurt();
    void PlayDeath();
}
