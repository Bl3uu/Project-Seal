using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float defaultMoveSpeed = 5f;

    private EntityMotor playerMotor;
    private PlayerDash playerDash;
    private PlayerAttack playerAttack;
    private Vector2 moveDirection;

    private void Awake()
    { 
        playerMotor = GetComponent<EntityMotor>();
        playerDash = GetComponent<PlayerDash>();
        playerAttack = GetComponent<PlayerAttack>();
    }

    private void Update()
    {
        moveDirection = ReadInputVector();
    }

    private void FixedUpdate()
    {
        if ((playerDash != null && playerDash.IsDashing) || (playerAttack != null && playerAttack.IsAttacking))
        {
            return;
        }

        playerMotor.Move(moveDirection, defaultMoveSpeed);
    }

    private Vector2 ReadInputVector()
    {
        float x = 0f;
        float y = 0f;

        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
        }

        return new Vector2(x, y);
    }
    
}