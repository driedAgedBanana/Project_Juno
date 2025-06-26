using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    public enum EnemyBehaviour
    {
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

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        currentEnemyBehaviour = EnemyBehaviour.Patrolling;
    }

    protected virtual void Update()
    {
        HandleState();
    }

    protected abstract void HandleState();

    public virtual void TakeDamage(int damageAmount)
    {
        if (currentEnemyBehaviour == EnemyBehaviour.Die)
        {
            return;
        }

        currentHealth -= damageAmount;
        enemyAnimator.SetTrigger("isHurt");
        currentEnemyBehaviour = EnemyBehaviour.Hurt;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    protected virtual void Die()
    {
        currentEnemyBehaviour = EnemyBehaviour.Die;
        enemyAnimator.SetTrigger("die");
        Destroy(gameObject, dieLifeTime);
    }
}
