using System.Collections;
using UnityEngine;
using UnityEngineInternal;

public class Enemy_script : Enemy_health
{
    public Animator enemyAnimator;

    [Header("Movement speed")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.5f;
    private float currentSpeed;
    private bool _shouldStop = false;
    private bool _movingRight = true;

    [Header("Detection")]
    public float detectionRange = 5f;
    public float patrolCheckDistance;
    public float groundDetectionDistance = 2f;
    public LayerMask groundLayer;
    public Transform patrolCheckFront;
    public LayerMask enemyLayer;

    [Header("References")]
    public Transform groundDetection;
    public Transform player;
    public Rigidbody2D rb2D;


    [Header("Attack and damages")]
    private bool _isAttacking;
    public float idleBeforeAttack = 1f;
    public float attackDuration = 0.5f;
    public float strikeTime;
    public float cooldownBetweenAttacks = 1f;
    public int attackRange;

    private Coroutine _attackCoroutine;
    [SerializeField] private bool _isPlayerInRange = false;
    [SerializeField] private bool _canAttack = false;
    public LayerMask playerLayer;
    public int damageAmount = 30;

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Player not found! Make sure your player has the 'Player' tag assigned.");
        }

        _canAttack = false;
        _isAttacking = false;

        rb2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (_shouldStop || _isAttacking) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRange)
        {
            ChasingPlayer();
        }
        else
        {
            Patrolling();
        }
    }

    private void Patrolling()
    {
        if (_isAttacking) return;

        currentSpeed = patrolSpeed;
        enemyAnimator.SetBool("isChasingPlayer", false);

        // Move left or right depending on facing direction
        transform.Translate((_movingRight ? Vector2.right : Vector2.left) * currentSpeed * Time.deltaTime);

        // Raycast downward to check for ground
        RaycastHit2D groundInfo = Physics2D.Raycast(groundDetection.position, Vector2.down, groundDetectionDistance, groundLayer);

        // Raycast forward to check if other enemies are around
        Vector2 frontDirection = _movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D patrolCheckHit = Physics2D.Raycast(patrolCheckFront.position, frontDirection, patrolCheckDistance, enemyLayer);
        Debug.DrawRay(patrolCheckFront.position, patrolCheckDistance * frontDirection, Color.green);

        // If no ground detected, flip direction
        if (!groundInfo.collider || (patrolCheckHit.collider != null))
        {
            Flip();
        }
    }

    private void ChasingPlayer()
    {
        currentSpeed = chaseSpeed;
        enemyAnimator.SetBool("isChasingPlayer", true);

        // Direction vector normalized toward player
        Vector2 direction = new Vector2(player.position.x - transform.position.x, 0f).normalized;

        // Move toward the player
        Vector2 targetPos = new Vector2(player.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);

        // Flip sprite to face movement direction
        if ((direction.x > 0 && !_movingRight) || (direction.x < 0 && _movingRight))
        {
            Flip();
        }
    }

    private void Flip()
    {
        _movingRight = !_movingRight;

        // Flip visual sprite by inverting scale.x
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _shouldStop = true;
            currentSpeed = 0;
            _isPlayerInRange = true;

            _attackCoroutine = StartCoroutine(Attacking());
        }
    }

    private IEnumerator Attacking()
    {
        _canAttack = true;
        _isAttacking = true;

        rb2D.linearVelocity = Vector2.zero;
        while (_isPlayerInRange)
        {
            animator.Play("Idle");
            yield return new WaitForSeconds(idleBeforeAttack);

            if (!_isPlayerInRange) break;


            animator.Play("Attack");

            yield return new WaitForSeconds(strikeTime);

            if (player != null && Vector2.Distance(transform.position, player.position) < attackRange)
            {
                Debug.Log("Apply damage and knockback on player!");
                player.GetComponent<Player_health>().TakeDamage(damageAmount);
                player.GetComponent<Player_health>().ApplyKnockBack(transform.position);
            }

            yield return new WaitForSeconds(attackDuration - strikeTime);
            if (!_isPlayerInRange) break;

            animator.Play("Idle");
            yield return new WaitForSeconds(cooldownBetweenAttacks);
        }

        animator.Play("Idle");
        _canAttack = false;
        _isAttacking = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }

            if (!_isAttacking || player != null && Vector2.Distance(transform.position, player.position) < detectionRange)
            {
                ChasingPlayer();
            }
            else
            {
                Patrolling();
            }
        }
    }
}
