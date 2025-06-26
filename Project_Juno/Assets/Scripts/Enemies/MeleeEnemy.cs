using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    [Header("Movement")]
    public float patrolSpeed = 4f;
    public float chasingSpeed = 6f;
    public float attackingRange = 3f;
    public float attackingCoolDown = 2f;

    private float _currentSpeed;

    private float _lastAttackTime;
    private bool _isMovingRight = true;

    public Transform groundCheck;
    public Transform patrolCheckFront;
    public LayerMask groundLayer;
    public LayerMask enemyLayer;

    public float groundDetectionDistance;
    public float patrolCheckDistance;

    protected override void HandleState()
    {
        
    }

    private void Patrolling()
    {
        float move = _isMovingRight ? 1 : -1;
        transform.Translate(Vector2.right * move * patrolSpeed * Time.deltaTime);

        // Flip when hitting egde
        RaycastHit2D groundInfo = Physics2D.Raycast(groundCheck.position, Vector2.down, groundDetectionDistance, groundLayer);

        // Raycast forward to check if other enemies are around
        Vector2 frontDirection = _isMovingRight ? Vector2.right : Vector2.left;
        RaycastHit2D patrolCheckHit = Physics2D.Raycast(patrolCheckFront.position, frontDirection, patrolCheckDistance, enemyLayer);
        Debug.DrawRay(patrolCheckFront.position, patrolCheckDistance * frontDirection, Color.green);

        if (!groundInfo.collider)
        {
            // Flip
        }
    }
}
