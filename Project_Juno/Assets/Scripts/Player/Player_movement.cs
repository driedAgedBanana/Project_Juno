using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class Player_movement : MonoBehaviour
{
    public static Player_movement Instance;

    [Header("Animations")]
    public Animator playerAnimator;

    [Header("Movement")]
    public Rigidbody2D rb2D;
    public float moveSpeed = 8f;
    [HideInInspector] public float currentmoveSpeed;
    [HideInInspector] public float horizontalMovement;
    [HideInInspector] public bool isPlayerFacingRight = true;

    [Header("Jump")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float jumpingForce = 8;

    [Header("Dashing")]
    public TrailRenderer dashingTrail;
    [HideInInspector] public bool canDash = true;
    [HideInInspector] public bool isDashing;
    public float dashingPower = 20f;
    public float dashingTime = 0.2f;
    public float dashingCooldown = 1f;

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

        playerAnimator = GetComponent<Animator>();

        currentmoveSpeed = moveSpeed;
        dashingTrail.emitting = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Player_health.Instance.isAlive)
        {
            if (!isDashing)
            {

                dashingTrail.emitting = false;

                rb2D.linearVelocity = new Vector2(horizontalMovement * currentmoveSpeed, rb2D.linearVelocity.y);

                if (!isPlayerFacingRight && horizontalMovement > 0)
                {
                    FlipCharacter();
                }
                else if (isPlayerFacingRight && horizontalMovement < 0)
                {
                    FlipCharacter();
                }

                playerAnimator.SetFloat("xVelocity", Mathf.Abs(rb2D.linearVelocity.x));
                playerAnimator.SetFloat("yVelocity", rb2D.linearVelocity.y);
                playerAnimator.SetBool("isJumping", !IsGrounded());
            }
            else
            {
                playerAnimator.SetTrigger("isDashing");
                return;
            }
        }
        else
        {
            return;
        }

    }

    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && IsGrounded())
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, jumpingForce);
        }

        if (ctx.canceled && rb2D.linearVelocity.y > 0)
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

    public void Dashing(InputAction.CallbackContext ctx)
    {
        if (ctx.started && canDash)
        {
            StartCoroutine(Dashing());
        }
    }

    private IEnumerator Dashing()
    {
        canDash = false;

        isDashing = true;
        float originalGravity = rb2D.gravityScale; // Remember the original gravity because the RB will be disabled for a brief moment
        rb2D.gravityScale = 0f;
        rb2D.linearVelocity = new Vector2(horizontalMovement * dashingPower, 0f);
        dashingTrail.emitting = true;
        yield return new WaitForSeconds(dashingTime);

        rb2D.gravityScale = originalGravity;
        isDashing = false;
        dashingTrail.emitting = false;

        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}
