using System.Runtime.InteropServices;
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

    [Header("Attacking properties")]
    public int damageAmount;

    protected override void HandleState()
    {
        switch (currentEnemyBehaviour)
        {
            case EnemyBehaviour.Idling:
                if (Vector2.Distance(transform.position, player.position) > detectionRange)
                {
                    currentEnemyBehaviour = EnemyBehaviour.Patrolling;
                }
                else
                {
                    currentEnemyBehaviour = EnemyBehaviour.Chasing;
                }
                break;

            case EnemyBehaviour.Patrolling:
                Patrolling();
                if (Vector2.Distance(transform.position, player.position) < detectionRange)
                {
                    currentEnemyBehaviour = EnemyBehaviour.Chasing;
                }
                break;

            case EnemyBehaviour.Chasing:
                ChasingPlayer();
                break;

            case EnemyBehaviour.Attacking:
                // coming son
                break;

            case EnemyBehaviour.Hurt:
                // Coming soon
                break;

            case EnemyBehaviour.Die:
                break;
        }
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

        if (!groundInfo.collider || (patrolCheckHit.collider != null))
        {
            Flip();
        }
    }

    private void ChasingPlayer()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackingRange && Time.time > _lastAttackTime + attackingCoolDown)
        {
            currentEnemyBehaviour = EnemyBehaviour.Attacking;
            enemyAnimator.SetTrigger("isAttacking");
            _lastAttackTime = Time.time;
        }
        else
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.Translate(direction * chasingSpeed * Time.deltaTime);
        }
    }

    public void DealingDamage() // Call it via an animation event key
    {
        if (Vector2.Distance(transform.position, player.position) <= attackingRange)
        {
            player.GetComponent<Player_health>().TakeDamage(damageAmount);
        }

        currentEnemyBehaviour = EnemyBehaviour.Idling;
    }

    void Flip()
    {
        _isMovingRight = !_isMovingRight;

        // Flip visual sprite by inverting scale.x
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }
}
