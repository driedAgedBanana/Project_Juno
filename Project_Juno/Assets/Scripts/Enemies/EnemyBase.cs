using System.Collections;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    public enum EnemyBehaviour
    {
        Idling,
        Patrolling,
        Chasing,
        Attacking,
        Hurt,
        Die
    }

    protected EnemyBehaviour currentEnemyBehaviour;

    public int maxHealth;
    protected int currentHealth;

    [Header("General Setup")]
    public float detectionRange = 5f;
    public Transform player;
    public Animator enemyAnimator;
    public float dieLifeTime;
    public Rigidbody2D rb2D;

    protected bool isHurt;
    public float hurtTime = 0.5f;

    protected Coroutine hurtResponse;

    [Header("Combat setting")]
    protected float attackingRange = 3f;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        isHurt = false;
        currentEnemyBehaviour = EnemyBehaviour.Patrolling;
    }

    protected virtual void Update()
    {
        if (currentEnemyBehaviour == EnemyBehaviour.Hurt || currentEnemyBehaviour == EnemyBehaviour.Die)
            return;

        HandleState();
    }


    protected abstract void HandleState();

    public virtual void TakeDamage(int damageAmount)
    {
        if (currentEnemyBehaviour == EnemyBehaviour.Die)
            return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (hurtResponse != null)
        {
            StopCoroutine(hurtResponse);
        }

        if (this is MeleeEnemy melee)
        {
            // Stop attacking coroutine if active
            if (melee.attackingCoroutine != null)
            {
                melee.StopCoroutine(melee.attackingCoroutine);
                melee.enemyAnimator.SetBool("isAttackingPlayer", false);
                melee.attackingCoroutine = null;
                melee._isAttacking = false;
            }
        }

        hurtResponse = StartCoroutine(Hurt());
    }


    protected IEnumerator Hurt()
    {
        isHurt = true;
        enemyAnimator.SetTrigger("isHurt");
        currentEnemyBehaviour = EnemyBehaviour.Hurt;

        yield return new WaitForSeconds(hurtTime);

        isHurt = false;

        // Decide what to do after hurt ends
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackingRange)
        {
            currentEnemyBehaviour = EnemyBehaviour.Attacking;
        }
        else if (distance <= detectionRange)
        {
            currentEnemyBehaviour = EnemyBehaviour.Chasing;
        }
        else
        {
            currentEnemyBehaviour = EnemyBehaviour.Patrolling;
        }
    }


    protected virtual void Die()
    {
        if (this is MeleeEnemy melee)
        {
            melee.currentSpeed = 0;
        }
        currentEnemyBehaviour = EnemyBehaviour.Die;
        enemyAnimator.SetTrigger("isDead");
        Destroy(gameObject, dieLifeTime);
    }
}
