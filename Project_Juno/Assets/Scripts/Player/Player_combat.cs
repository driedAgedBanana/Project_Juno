using UnityEngine;
using UnityEngine.InputSystem;

public class Player_combat : MonoBehaviour
{
    public static Player_combat Instance;

    [HideInInspector] public bool isAttacking;

    public Animator playerAnimator;

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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Attack(InputAction.CallbackContext ctx)
    {
        if(ctx.started)
        {
            isAttacking = true;
            playerAnimator.SetTrigger("isAttacking");
        }
    }
}
