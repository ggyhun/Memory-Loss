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
    private bool PreviouslyAttacked = false;
    
    private void Awake()
    {
        mover = GetComponent<EnemyMover>();                     // 같은 오브젝트에 붙이기
        var playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerStats = playerObject.GetComponent<Stats>();
            playerController = playerObject.GetComponent<PlayerController>();
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
            if (PreviouslyAttacked || playerController.spells.Count >= 4)
            {
                // 주문 봉인 코드
            }
            else
            {
                playerStats.TakeDamage(attackDamage);
            }
            return;
        }
        
        // 봉인 범위 체크
        var grid = FindFirstObjectByType<GridManager>();
        var enemyCell  = grid.WorldToCell(enemy.transform.position);
        var playerCell = grid.WorldToCell(player.position);
        var d = playerCell - enemyCell;
        int dist2 = d.x * d.x + d.y * d.y;

        if (dist2 <= detectionRange * detectionRange && playerController.spells.Count >= 4)
        {
            // 주문 봉인 코드
        }
        else
        {
            mover.TryStepTowardTarget(enemy.gameObject, player.gameObject);
        }
    }
}
