using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_combat : MonoBehaviour
{
    public static Player_combat Instance;

    [HideInInspector] public bool isAttacking;

    public Animator playerAnimator;

    public float attackRate = 2f;
    private float _nextAtkTime;
    public float waitTime;
    public int attackIndex = 0;

    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;

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

        playerAnimator = GetComponent<Animator>();
        isAttacking = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Attack(InputAction.CallbackContext ctx)
    {
        if(ctx.started && Time.time >= _nextAtkTime)
        {
            StartCoroutine(Attacking());
            _nextAtkTime = Time.time + 1f / attackRate;
        }
    }

    private IEnumerator Attacking()
    {
        isAttacking = true;
        Player_movement.Instance.currentmoveSpeed = 0;
        attackIndex = (attackIndex + 1) % 2;
        playerAnimator.SetInteger("attackIndex", attackIndex);
        playerAnimator.SetTrigger("isAttacking");
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach(Collider2D enemies in hitEnemies)
        {
            Debug.Log("Hit " + enemies.name);
        }

        yield return new WaitForSeconds(waitTime);
        isAttacking = false;
        Player_movement.Instance.currentmoveSpeed = Player_movement.Instance.moveSpeed;
    }

    private void OnDrawGizmosSelected()
    {
        if(attackPoint == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
