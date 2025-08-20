using UnityEngine;
using System.Collections.Generic;

public class SlimBehavior : EnemyBehavior
{
    [Header("Slim Behavior Settings")]
    public int detectionRange = 5; // 칸 단위 시야
    
    [Header("Slim Attack Settings")]
    public int attackDamage = 5; // 공격력
    public GameObject attackAreaPrefab; // 공격 범위 프리팹
    public Stats stats;

    private EnemyMover mover;
    private GameObject playerObject;
    private Transform player;
    private Stats playerStats;
    private EnemyAttackArea attackArea; // 공격 범위 컴포넌트
    
    private void Awake()
    {
        mover = GetComponent<EnemyMover>();                     // 같은 오브젝트에 붙이기
        var playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerStats = playerObject.GetComponent<Stats>();
        }
        
        attackArea = attackAreaPrefab.GetComponent<EnemyAttackArea>();
        attackArea.transform.SetParent(transform); // 공격 범위 오브젝트를 슬림 오브젝트의 자식으로 설정
        attackArea.transform.localPosition = Vector3.zero; // 위치 초기화
    }

    public override void Act(Enemy enemy)
    {
        if (mover == null || player == null) return;
        
        if (!enemy.GetComponent<Stats>().CanAct) return; // 행동 불가 시 종료

        if (attackArea != null && attackArea.CanAttack())
        {
            playerStats.TakeDamage(attackDamage);
            return;
        }
        
        // 시야 체크
        var grid = FindFirstObjectByType<GridManager>();
        var enemyCell  = grid.WorldToCell(enemy.transform.position);
        var playerCell = grid.WorldToCell(player.position);
        var d = playerCell - enemyCell;
        int dist2 = d.x * d.x + d.y * d.y;

        if (dist2 <= detectionRange * detectionRange)
        {
            // 보이면 플레이어 쪽으로 1칸
            mover.TryStepTowardTarget(enemy.gameObject, player.gameObject);
        }
        else
        {
            // 안 보이면 상/하/좌/우 랜덤 1칸 (막히면 이동X)
            mover.TryRandomCardinalStep(enemy.gameObject);
        }
    }
}
