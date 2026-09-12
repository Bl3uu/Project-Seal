using UnityEngine;
using UnityEngine.UIElements;

public class PlayerAnimation : MonoBehaviour
{
    private PlayerMovement movement;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Vector2 lastFacingDirection = Vector2.down;

    private void Awake()
    {
        movement= GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        bool isMoving = movement.IsMoving;
        Vector2 input = movement.MoveDirection;

        animator.SetBool("IsMoving", isMoving);

        if (isMoving)
        {
            // Horizontal Directions (Left/Right Priority)
            if (Mathf.Abs(input.x) >= Mathf.Abs(input.y))
            {
                lastFacingDirection = new Vector2(Mathf.Sign(input.x), 0f);

                if (input.x < 0f) 
                {
                    spriteRenderer.flipX = true; // Flip sprite horizontally when moving left
                }
                else if (input.x > 0f)
                { 
                    spriteRenderer.flipX = false; // Flip sprite false if moving right
                }
            }
            // Vertical Directions (Up/Down)
            else
            {
                lastFacingDirection = new Vector2(0f, Mathf.Sign(input.y));
                spriteRenderer.flipX= false; // Reset flip for cardinal Up/Down
            }
        }
        // Pass last facing direction to Animator parameters
        animator.SetFloat("MoveX", lastFacingDirection.x);
        animator.SetFloat("MoveY", lastFacingDirection.y);
    }
}
