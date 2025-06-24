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
    private bool _pauseAfterAttack = false;
    public float pauseDurationAfterAttack = 0.75f;

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
        if ( _isAttacking) return;

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
        if (_isAttacking) return;

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
            currentSpeed = 0;
            _isPlayerInRange = true;

            _attackCoroutine = StartCoroutine(Attacking());
        }
    }

    private IEnumerator Attacking()
    {
        _isAttacking = true;
        rb2D.linearVelocity = Vector2.zero;
        enemyAnimator.Play("Idle");

        yield return new WaitForSeconds(idleBeforeAttack);

        if (!_isPlayerInRange)
        {
            _isAttacking = false;
            yield break;
        }

        enemyAnimator.SetTrigger("isAttackingPlayer");
        yield return new WaitForSeconds(strikeTime);

        if (player != null && Vector2.Distance(transform.position, player.position) < attackRange)
        {
            Debug.Log("Apply damage and knockback on player!");
            player.GetComponent<Player_health>().TakeDamage(damageAmount);
            player.GetComponent<Player_health>().ApplyKnockBack(transform.position);

            StartCoroutine(PauseAfterAttack());
        }

        yield return new WaitForSeconds(attackDuration - strikeTime);

        enemyAnimator.Play("Idle");
        _isAttacking = false;

        yield return new WaitForSeconds(cooldownBetweenAttacks);
    }

    private IEnumerator PauseAfterAttack()
    {
        _pauseAfterAttack = true;
        yield return new WaitForSeconds(pauseDurationAfterAttack);
        _pauseAfterAttack = false;
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _isPlayerInRange = false;
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }
        }
    }
}
