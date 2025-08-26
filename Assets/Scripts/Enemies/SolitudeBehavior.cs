using System;
using UnityEngine;
using System.Collections.Generic;

public class SolitudeBehavior : EnemyBehavior
{
    [Header("Solitude Behavior Settings")]
     public int detectionRange = 4; // 칸 단위 모방 범위
    
    [Header("Solitude Attack Settings")]
    // public int attackDamage = 5; // 공격력
    public GameObject attackAreaPrefab; // 공격 범위 프리팹
    public Stats stats;

    private EnemyMover mover;
    private GameObject playerObject;
    private Transform player;
    private Stats playerStats;
    private EnemyAttackArea attackArea; // 공격 범위 컴포넌트
    private GridManager grid;
    private Stats myStats;
    
    private void Awake()
    {
        mover = GetComponent<EnemyMover>();
        myStats = GetComponent<Stats>();
        grid = FindFirstObjectByType<GridManager>();

        var pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null)
        {
            player = pObj.transform;
            playerStats = pObj.GetComponent<Stats>();
        }
        else
        {
            Debug.LogWarning($"{name}: Player not found.");
        }

        if (attackAreaPrefab != null)
        {
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

        if (attackArea != null && attackArea.CanAttack() && PlayerMoveRecorder.Instance.GetPreviousMove() == PlayerMoveType.Attack)
        {
            // 데미지만 복사하는 방식이라 플레이어 공격 범위 내 + 이전 턴 플레이어 행동이 공격 조건으로 적용
            playerStats.TakeDamage((int)(PlayerMoveRecorder.Instance.GetSpellDamage() * 0.7));
            return;
        }

        // 모방 범위 체크
        var enemyCell  = grid.WorldToCell(enemy.transform.position);
        var playerCell = grid.WorldToCell(player.position);
        var d = playerCell - enemyCell;
        
        // x, y 좌표의 절댓값을 각각 구하고, 둘 중 큰 값을 사용
        int dist2 = Math.Max(Math.Abs(d.x), Math.Abs(d.y));

        if (dist2 >= detectionRange)
        {
            // 모방 범위 밖이면 플레이어 쪽으로 1칸
            mover.TryStepTowardTarget(enemy.gameObject, player.gameObject);
        }
        else
        {
            // 플레이어 이동시 반대 방향으로 이동 시도
            if (PlayerMoveRecorder.Instance.GetPreviousMove() == PlayerMoveType.Up)
            {
                mover.TryMoveToward(enemy.gameObject, 1);
                return;
            }
            else if (PlayerMoveRecorder.Instance.GetPreviousMove() == PlayerMoveType.Down)
            {
                mover.TryMoveToward(enemy.gameObject, 0);
                return;
            }
            else if (PlayerMoveRecorder.Instance.GetPreviousMove() == PlayerMoveType.Left)
            {
                mover.TryMoveToward(enemy.gameObject, 3);
                return;
            }
            else if (PlayerMoveRecorder.Instance.GetPreviousMove() == PlayerMoveType.Right)
            {
                mover.TryMoveToward(enemy.gameObject, 2);
                return;
            }
            else if (dist2 > 1) // 플레이어가 이동하지 않았고 거리가 1칸보다 멀면 플레이어 방향으로 이동
            {
                mover.TryStepTowardTarget(enemy.gameObject, player.gameObject);
            }
        }
    }
}
