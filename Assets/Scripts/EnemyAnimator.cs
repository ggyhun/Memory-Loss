using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    [Header("Animator Settings")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public bool isTowardRight = true; // 기본 방향은 오른쪽
    
    public string idleState = "Idle";
    public string moveState = "Move";
    public string attackState = "Attack";
    public string hurtState = "Hurt";
    public string deathState = "Die";

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError($"{name}: Animator component not found.");
            }
        }
    }

    public void PlayIdle()
    {
        animator.Play(idleState);
    }

    public void PlayMove(Vector3Int moveDir)
    {
        if (moveDir.x > 0) isTowardRight = true;
        else if (moveDir.x < 0) isTowardRight = false;
        animator.SetTrigger(moveState);
    }

    public void PlayAttack(Vector3Int attackDir)
    {
        if (attackDir != Vector3Int.zero)
        {
            if (attackDir.x > 0) isTowardRight = true;
            else if (attackDir.x < 0) isTowardRight = false;
        }
        
        animator.SetTrigger(attackState);
    }

    public void PlayHurt()
    {
        animator.SetTrigger(hurtState);
    }

    public void PlayDeath()
    {
        animator.SetTrigger(deathState);
    }
}