using System;
using UnityEngine;

[RequireComponent(typeof(EnemyMover))]
[RequireComponent(typeof(Stats))]
public class AnxietyBehavior : EnemyBehavior
{
    /// <summary>
    /// 이동방식 : 플레이어를 향해 한 칸 이동 (시야 기믹 없음, 자동 추적) <br></br>
    /// 공격방식 : 2칸 이내 공격 <br></br>
    /// 특수패턴 : 피격시 2턴간 투명화 <br></br>
    /// </summary>
    
    [Header("Anxiety Behavior Settings")]
    public int invisibleTurns = 2; // 투명화 지속 턴 수
    public SpriteRenderer spriteRenderer;
    private int invisibleTurnsLeft = 0; // 남은 투명화 턴 수
    
    [Header("Anxiety Attack Settings")]
    public int attackDamage = 5;
    public GameObject attackAreaPrefab; // 반드시 프리팹(씬 인스턴스 아님)

    private EnemyMover mover;
    private Transform player;       // 필드만 사용
    private Stats playerStats;
    private EnemyAttackArea attackArea;
    private GridManager grid;
    private Stats myStats;
    private int myPreviousHealth;
    
    private void Awake()
    {
        mover    = GetComponent<EnemyMover>();
        myStats  = GetComponent<Stats>();
        grid     = FindFirstObjectByType<GridManager>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        var pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) {
            player      = pObj.transform;
            playerStats = pObj.GetComponent<Stats>();
        } else {
            Debug.LogWarning($"{name}: Player not found.");
        }

        if (attackAreaPrefab != null) {
            // ✅ 프리팹을 인스턴스화해서 자식으로 부착
            var inst = Instantiate(attackAreaPrefab, transform);
            attackArea = inst.GetComponent<EnemyAttackArea>();
            if (attackArea == null)
                Debug.LogWarning($"{name}: EnemyAttackArea component not found on attackAreaPrefab.");
        }
    }
    
    public override void Act(Enemy enemy)
    {
        if (mover == null || player == null || grid == null) return;
        if (!myStats.CanAct) return;
        
        // 투명화 상태 처리
        if (IsInvisible())
        {
            invisibleTurnsLeft--;
            if (invisibleTurnsLeft <= 0)
            {
                // 투명화 종료
                invisibleTurnsLeft = 0;
                myStats.ResetInvincible();
                if (spriteRenderer != null)
                {
                    // 투명화 해제(완전 불투명)
                    var color = spriteRenderer.color;
                    color.a = 1f;
                    spriteRenderer.color = color;
                }
            }
        }
        
        if (IsDamaged())
        {
            // 이미 투명화 상태면 무시
            if (!IsInvisible())
            {
                // 투명화 시작
                invisibleTurnsLeft = invisibleTurns;
                myStats.SetInvincible();
                if (spriteRenderer != null)
                {
                    // 투명화(완전 투명)
                    var color = spriteRenderer.color;
                    color.a = 0f;
                    spriteRenderer.color = color;
                }
            }
        }
        
        // 공격 가능이면 공격 먼저
        if (attackArea != null && attackArea.CanAttack())
        {
            // 공격 애니메이션 재생
            mover.enemyAnimator.PlayAttack(Vector3Int.zero);
            playerStats?.TakeDamage(attackDamage);
        }
        else
        {
            // 이동
            mover.TryStepTowardTarget(enemy.gameObject, player.gameObject);
        }
        
        myPreviousHealth = myStats.currentHp;
    }
    
    private bool IsDamaged() => myStats.currentHp < myPreviousHealth;
    
    private bool IsInvisible() => invisibleTurnsLeft > 0;
}
