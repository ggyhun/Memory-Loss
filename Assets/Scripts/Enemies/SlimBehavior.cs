using System;
using UnityEngine;

[RequireComponent(typeof(EnemyMover))]
[RequireComponent(typeof(Stats))]
public class SlimBehavior : EnemyBehavior
{
    [Header("Slim Behavior Settings")]
    public int detectionRange = 5; // 칸 단위 시야

    [Header("Slim Attack Settings")]
    public int attackDamage = 5;
    public GameObject attackAreaPrefab; 

    private EnemyMover mover;
    private Transform player;       // 필드만 사용
    private Stats playerStats;
    private EnemyAttackArea attackArea;
    private GridManager grid;
    private Stats myStats;

    private void Awake()
    {
        mover    = GetComponent<EnemyMover>();
        myStats  = GetComponent<Stats>();
        grid     = FindFirstObjectByType<GridManager>();

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

        // 공격 가능이면 공격 먼저
        if (attackArea != null && attackArea.CanAttack())
        {
            // 공격 애니메이션 재생
            mover.enemyAnimator.PlayAttack(Vector3Int.zero);
            playerStats?.TakeDamage(attackDamage);
            return;
        }
        
        var enemyCell  = grid.WorldToCell(enemy.transform.position);
        var playerCell = grid.WorldToCell(player.position);
        var d = playerCell - enemyCell;
        
        // x, y 좌표의 절댓값을 각각 구하고, 둘 중 큰 값을 사용
        // 이 값이 detectionRange 이하이면 플레이어를 향해 이동
        int dist2 = Math.Max(Math.Abs(d.x), Math.Abs(d.y));

        if (dist2 <= detectionRange) {
            mover.TryStepTowardTarget(enemy.gameObject, player.gameObject);
        } else {
            mover.TryRandomCardinalStep(enemy.gameObject);
        }
    }
}
