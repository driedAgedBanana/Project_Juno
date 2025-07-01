using System.Collections;
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

    private bool _isMovingRight = true;

    public Transform groundCheck;
    public Transform patrolCheckFront;
    public LayerMask groundLayer;
    public LayerMask enemyLayer;

    public float groundDetectionDistance;
    public float patrolCheckDistance;

    [Header("Attacking properties")]
    private bool _isAttacking = false;
    private Coroutine _attackCoroutine;
    public int damageAmount;

    public float attackBufferZone = 0.5f; // prevent animation flickering
    public float minAttackCommitTime = 0.4f;

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
                if (Vector2.Distance(transform.position, player.position) >= detectionRange)
                {
                    Patrolling();
                }
                else
                {
                    currentEnemyBehaviour = EnemyBehaviour.Chasing;
                }
                break;

            case EnemyBehaviour.Chasing:
                ChasingPlayer();
                break;

            case EnemyBehaviour.Attacking:
                AttackingPlayer();
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
        _currentSpeed = patrolSpeed;
        transform.Translate(Vector2.right * move * _currentSpeed * Time.deltaTime);

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
        _currentSpeed = chasingSpeed;

        // Use buffer zone to prevent rapid toggling at edge of range
        if (distance <= attackingRange - attackBufferZone)
        {
            enemyAnimator.SetBool("isChasingPlayer", false);
            currentEnemyBehaviour = EnemyBehaviour.Attacking;
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;
        transform.Translate(direction * _currentSpeed * Time.deltaTime);

        enemyAnimator.SetBool("isChasingPlayer", true);

        if(Vector2.Distance(transform.position, player.position) >= detectionRange)
        {
            enemyAnimator.SetBool("isChasingPlayer", false);
            currentEnemyBehaviour = EnemyBehaviour.Patrolling;
            return;
        }

        if ((direction.x > 0 && !_isMovingRight) || (direction.x < 0 && _isMovingRight))
        {
            Flip();
        }
    }


    private void AttackingPlayer()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > detectionRange)
        {
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }
            _isAttacking = false;
            enemyAnimator.SetBool("isAttackingPlayer", false);
            currentEnemyBehaviour = EnemyBehaviour.Patrolling;
            return;
        }

        // Only exit attack when player is BEYOND buffer zone
        if (distance > attackingRange + attackBufferZone && !_isAttacking)
        {
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }
            enemyAnimator.SetBool("isAttackingPlayer", false);
            currentEnemyBehaviour = EnemyBehaviour.Chasing;
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;
        if ((direction.x > 0 && !_isMovingRight) || (direction.x < 0 && _isMovingRight))
            Flip();

        if (!_isAttacking)
        {
            _attackCoroutine = StartCoroutine(Attacking());
        }
    }

    private IEnumerator Attacking()
    {
        _isAttacking = true;
        enemyAnimator.SetBool("isAttackingPlayer", true);

        // Commit to attack for minimum time
        float attackTimer = 0;
        while (attackTimer < minAttackCommitTime)
        {
            attackTimer += Time.deltaTime;
            yield return null;
        }

        // Complete the remaining cooldown
        float remainingCooldown = attackingCoolDown - minAttackCommitTime;
        if (remainingCooldown > 0)
        {
            yield return new WaitForSeconds(remainingCooldown);
        }

        _isAttacking = false;
        enemyAnimator.SetBool("isAttackingPlayer", false);

        // Decide next state based on current distance
        float currentDistance = Vector2.Distance(transform.position, player.position);
        if (currentDistance <= attackingRange + attackBufferZone)
        {
            currentEnemyBehaviour = EnemyBehaviour.Attacking;
        }
        else
        {
            currentEnemyBehaviour = EnemyBehaviour.Chasing;
        }

        _attackCoroutine = null;
    }

    public void DealingDamage(int damageAmount) // Call it via an animation event key
    {
        if (Vector2.Distance(transform.position, player.position) <= attackingRange)
        {
            player.GetComponent<Player_health>().TakeDamage(damageAmount);
        }
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
