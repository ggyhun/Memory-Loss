using UnityEngine;
using UnityEngine.Scripting;

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

        CacheRefs();
    }

    private void OnEnable()  => CacheRefs();
    private void Start()     => CacheRefs();

    private void CacheRefs()
    {
        if (!animator)
            animator = GetComponentInChildren<Animator>(true);          // ✅ 자식까지 검색
        if (!spriteRenderer)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        // PlayerController에 할당된 Animator가 있다면 그것도 폴백으로 사용
        if (!animator && PlayerController.Instance && PlayerController.Instance.animator)
            animator = PlayerController.Instance.animator;

        if (!animator)
            Debug.LogError("PlayerAnimator: Animator not found in self or children.");
        if (!spriteRenderer)
            Debug.LogWarning("PlayerAnimator: SpriteRenderer not found in self or children.");
    }

    public void SetDirection(Vector2 dir)
    {
        if (!spriteRenderer) { CacheRefs(); if (!spriteRenderer) return; }
        if      (dir.x > 0) spriteRenderer.flipX = false;
        else if (dir.x < 0) spriteRenderer.flipX = true;
    }

    public void PlayAttackAnimation()
    {
        TurnManager.Instance.RequestEndAfterPlayerAnimation();

        if (!animator) { CacheRefs(); if (!animator) return; }
        animator.ResetTrigger("Move");
        animator.SetTrigger("Attack");
    }

    public void PlayerMoveAnimation()
    {
        // 이동은 턴 넘김 예약 안함
        if (!animator) { CacheRefs(); if (!animator) return; }
        animator.ResetTrigger("Attack");
        animator.SetTrigger("Move");
    }

    public void PlayHitAnimation()
    {
        if (!animator) { CacheRefs(); if (!animator) return; }
        animator.SetTrigger("Hurt");
    }

    public void PlayDeathAnimation()
    {
        if (!animator) { CacheRefs(); if (!animator) return; }
        animator.SetTrigger("Die");
    }

    // Animation Event에서 호출
    [Preserve]
    public void AnimEvent_ActionEnd()
    {
        TurnManager.Instance.NotifyPlayerAnimationComplete();
    }
}
