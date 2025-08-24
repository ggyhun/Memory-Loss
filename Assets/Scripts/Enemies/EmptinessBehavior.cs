using System;
using UnityEngine;
using System.Collections.Generic;

public class EmptinessBehavior : EnemyBehavior
{
    [Header("Emptiness Behavior Settings")]
    public int detectionRange = 5; // 칸 단위 봉인 범위
    
    [Header("Emptiness Attack Settings")]
    public int attackDamage = 8; // 공격력
    public GameObject attackAreaPrefab; // 공격 범위 프리팹
    public Stats stats;

    private EnemyMover mover;
    private GameObject playerObject;
    private Transform player;
    private Stats playerStats;
    private PlayerController playerController; // 주문서 개수 확인용
    private EnemyAttackArea attackArea; // 공격 범위 컴포넌트
    private GridManager grid;
    private Stats myStats;
    private bool PreviouslyAttacked = false;
    
    private void Awake()
    {
        mover = GetComponent<EnemyMover>();                     // 같은 오브젝트에 붙이기
        var playerObject = GameObject.FindGameObjectWithTag("Player");
        grid = FindFirstObjectByType<GridManager>();
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerStats = playerObject.GetComponent<Stats>();
        }

        attackArea = attackAreaPrefab.GetComponent<EnemyAttackArea>();
        attackArea.transform.SetParent(transform); // 공격 범위 오브젝트를 공허 오브젝트의 자식으로 설정
        attackArea.transform.localPosition = Vector3.zero; // 위치 초기화
    }

    public override void Act(Enemy enemy)
    {
        if (mover == null || player == null) return;
        
        if (!enemy.GetComponent<Stats>().CanAct) return; // 행동 불가 시 종료

        if (attackArea != null && attackArea.CanAttack())
        {
            if (PreviouslyAttacked)
            {
                if (!PlayerController.Instance.SealRandomSpell(4))
                {
                    playerStats.TakeDamage(attackDamage);
                }
            }
            else
            {
                playerStats.TakeDamage(attackDamage);
            }
            return;
        }
        
        // 봉인 범위 체크
        var enemyCell  = grid.WorldToCell(enemy.transform.position);
        var playerCell = grid.WorldToCell(player.position);
        var d = playerCell - enemyCell;
        
        // x, y 좌표의 절댓값을 각각 구하고, 둘 중 큰 값을 사용
        // 이 값이 detectionRange 이하이면 플레이어를 향해 이동
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
