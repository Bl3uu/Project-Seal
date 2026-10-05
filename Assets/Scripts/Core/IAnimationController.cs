using UnityEngine;

public interface IAnimationController
{
    void UpdateLocomotion(Vector2 movementInput, Vector2 lookDirection);
    void PlayerAttack(int comboStep, float aimAngle);
    void PlayHurt();
    void PlayDeath();
}
