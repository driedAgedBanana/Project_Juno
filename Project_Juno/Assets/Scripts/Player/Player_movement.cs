using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Player_movement : MonoBehaviour
{
    public static Player_movement Instance;

    [Header("Animations")]
    public Animator player_Animator;

    [Header("Movement")]
    public Rigidbody2D rb2D;
    public float move_Speed = 5f;
    [HideInInspector] public float horizontal_Movement;
    [HideInInspector] public bool is_Player_Facing_Right = true;

    [Header("Jump")]
    public Transform ground_Check;
    public LayerMask ground_Layer;
    public float jumping_Force = 8;

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

        player_Animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Player_health.Instance.isAlive)
        {
            rb2D.linearVelocity = new Vector2(horizontal_Movement * move_Speed, rb2D.linearVelocity.y);

            if (!is_Player_Facing_Right && horizontal_Movement > 0)
            {
                FlipCharacter();
            }
            else if (is_Player_Facing_Right && horizontal_Movement < 0)
            {
                FlipCharacter();
            }

            player_Animator.SetBool("isJumping", !IsGrounded());

            player_Animator.SetFloat("xVelocity", Mathf.Abs(rb2D.linearVelocity.x));
            player_Animator.SetFloat("yVelocity", rb2D.linearVelocity.y);

        }
        else
        {
            return;
        }

    }

    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(ground_Check.position, 0.2f, ground_Layer);
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && IsGrounded())
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, jumping_Force);
        }

        if (ctx.canceled && rb2D.linearVelocity.y > 0)
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, rb2D.linearVelocity.y * 0.5f);
        }

    }

    public void Movement(InputAction.CallbackContext ctx)
    {
        horizontal_Movement = ctx.ReadValue<Vector2>().x;
    }

    public void FlipCharacter()
    {
        is_Player_Facing_Right = !is_Player_Facing_Right;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }
}
