using UnityEngine;
using System.Collections;

public class Player_health : MonoBehaviour
{
    [Header("Health section")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] private int _currentHealth;
    protected bool isAlive = true;
    public Animator animator;
    public float lifeTime;

    public float knockBackForce;
    public float knockBackDuration;

    [HideInInspector] public bool isKnockedBack;

    private void Start()
    {
        _currentHealth = maxHealth;

        if (animator == null)
        {
            Debug.Log("No animator assigned on " + gameObject.name);
            return;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        _currentHealth -= damageAmount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);

        if (animator == null)
        {
            return;
        }
        else
        {
            animator.SetTrigger("isHurt"); 
        }

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void ApplyKnockBack(Vector2 attackerPos)
    {
        isKnockedBack = true;

        Vector2 knockDirection = (Vector2)(transform.position - (Vector3)attackerPos).normalized;
        Vector2 force = knockDirection * knockBackForce;

        Player_movement.Instance.rb2D.linearVelocity = Vector2.zero; // reset the velocity
        Player_movement.Instance.rb2D.AddForce(force, ForceMode2D.Impulse);

        StartCoroutine(ResetKnockbackAfterDelay());
    }

    private IEnumerator ResetKnockbackAfterDelay()
    {
        yield return new WaitForSeconds(knockBackDuration);
        isKnockedBack = false;
        Player_movement.Instance.rb2D.linearVelocity = Vector2.zero;
    }

    public void Healing(int healingAmount)
    {
        _currentHealth += healingAmount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);
    }

    public void Die()
    {
        if (animator == null)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            StartCoroutine(DieAnimationHandler());
        }
    }

    private IEnumerator DieAnimationHandler()
    {
        animator.SetTrigger("isDead");

        yield return new WaitForSeconds(lifeTime);

        gameObject.SetActive(false);
    }
}
