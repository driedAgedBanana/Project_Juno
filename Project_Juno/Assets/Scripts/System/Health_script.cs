using System.Collections;
using UnityEngine;

public class Health_script : MonoBehaviour
{
    [Header("Health section")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] private int _currentHealth;
    protected bool isAlive = true;
    public Animator animator;
    public float lifeTime;

    protected virtual void Start()
    {
        _currentHealth = maxHealth;

        if (animator == null)
        {
            Debug.Log("No animator assigned on " + gameObject.name);
            return;
        }
    }

    public virtual void TakeDamage(int damageAmount)
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

    public virtual void Healing(int healingAmount)
    {
        _currentHealth += healingAmount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);
    }

    public virtual void Die()
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
