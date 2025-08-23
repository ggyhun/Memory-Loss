using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public static PlayerAnimator Instance { get; private set; }
    
    [Header("References")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!animator)        animator = GetComponent<Animator>();
        if (!spriteRenderer)  spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetDirection(Vector2 direction)
    {
        if      (direction.x > 0) spriteRenderer.flipX = false;
        else if (direction.x < 0) spriteRenderer.flipX = true;
    }

    public void PlayAttackAnimation()
    {
        // ✅ 애니 끝나면 턴 넘기도록 예약
        TurnManager.Instance.RequestEndAfterPlayerAnimation();

        animator.SetTrigger("Attack");
    }
    
    public void PlayerMoveAnimation()
    {
        // ✅ 애니 끝나면 턴 넘기도록 예약
        TurnManager.Instance.RequestEndAfterPlayerAnimation();

        animator.SetTrigger("Move");
    }

    public void PlayHitAnimation()   => animator.SetTrigger("Hit");
    public void PlayDeathAnimation() => animator.SetTrigger("Death");

    // ===============================
    // ✅ Animation Event에서 호출될 함수
    // (Attack 클립의 마지막 프레임에 이 함수명을 이벤트로 넣어줘)
    // ===============================
    public void AnimEvent_ActionEnd()
    {
        TurnManager.Instance.NotifyPlayerAnimationComplete();
    }
}
