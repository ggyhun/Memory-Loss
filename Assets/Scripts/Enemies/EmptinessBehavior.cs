using System;
using UnityEngine;
using System.Collections.Generic;

public class EmptinessBehavior : EnemyBehavior
{
    [Header("Emptiness Behavior Settings")]
    public int detectionRange = 3; // 칸 단위 봉인 범위
    
    [Header("Emptiness Attack Settings")]
    public int attackDamage = 8; // 공격력
    public GameObject attackAreaPrefab; // 공격 범위 프리팹
    public Stats stats;

    private EnemyMover mover;
    private GameObject playerObject;
    private Transform player;
    private Stats playerStats;
    private EnemyAttackArea attackArea; // 공격 범위 컴포넌트
    private GridManager grid;
    private Stats myStats;
    private bool PreviouslyAttacked = false;
    
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
        
        if (!enemy.GetComponent<Stats>().CanAct) return; // 행동 불가 시 종료

        if (attackArea != null && attackArea.CanAttack())
        {
            if (PreviouslyAttacked)
            {
                if (!PlayerController.Instance.SealRandomSpell(4))
                {
                    playerStats.TakeDamage(attackDamage);
                    PreviouslyAttacked = true;
                }
                else
                {
                    PreviouslyAttacked = false;
                }
            }
            else
            {
                playerStats.TakeDamage(attackDamage);
                PreviouslyAttacked = true;
            }
            return;
        }
        
        // 봉인 범위 체크
        var enemyCell  = grid.WorldToCell(enemy.transform.position);
        var playerCell = grid.WorldToCell(player.position);
        var d = playerCell - enemyCell;
        
        // x, y 좌표의 절댓값을 각각 구하고, 둘 중 큰 값을 사용
        // 이 값이 detectionRange 이하이면 주문 봉인 시도
        int dist2 = Math.Max(Math.Abs(d.x), Math.Abs(d.y));

        if (dist2 <= detectionRange)
        {
            if (!PlayerController.Instance.SealRandomSpell(4))
            {
                mover.TryStepTowardTarget(enemy.gameObject, player.gameObject);
            }
        }
        else
        {
            mover.TryStepTowardTarget(enemy.gameObject, player.gameObject);
        }
    }
}
