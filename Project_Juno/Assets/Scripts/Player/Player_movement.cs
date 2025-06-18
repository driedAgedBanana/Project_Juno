using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Player_movement : MonoBehaviour
{
    public static Player_movement Instance;

    [Header("Movement")]
    public Rigidbody2D rb2D;
    public float moveSpeed = 5f;
    [HideInInspector] public float horizontalMovement;
    public SpriteRenderer playerRenderer;
    [HideInInspector] public bool isPlayerFacingRight = true;

    [Header("Jump")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float jumpingForce = 8;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
            Instance = null;
        }
        else
        {
            Instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        rb2D.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb2D.linearVelocity.y);

        if(!isPlayerFacingRight && horizontalMovement > 0)
        {
            FlipCharacter();
        }
        else if (isPlayerFacingRight && horizontalMovement < 0)
        {
            FlipCharacter();
        }
    }

    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if(ctx.performed && IsGrounded())
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, jumpingForce);
        }

        if(ctx.canceled && rb2D.linearVelocity.y > 0)
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, rb2D.linearVelocity.y * 0.5f);
        }
    }

    public void Movement(InputAction.CallbackContext ctx)
    {
        horizontalMovement = ctx.ReadValue<Vector2>().x;
    }

    public void FlipCharacter()
    {
        isPlayerFacingRight = !isPlayerFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }
}
