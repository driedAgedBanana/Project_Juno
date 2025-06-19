using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_combat : MonoBehaviour
{
    public static Player_combat Instance;

    [HideInInspector] public bool isAttacking;

    public Animator playerAnimator;

    public float waitTime;

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
        if(ctx.started)
        {
            StartCoroutine(Attacking());
        }
    }

    private IEnumerator Attacking()
    {
        isAttacking = true;
        Player_movement.Instance.currentmoveSpeed = 0;
        playerAnimator.SetTrigger("isAttacking");
        yield return new WaitForSeconds(waitTime);
        isAttacking = false;
        Player_movement.Instance.currentmoveSpeed = Player_movement.Instance.moveSpeed;
    }
}
