using UnityEngine;

public class SlimBehavior : EnemyBehavior
{
    public int detectionRange = 5; // 칸 단위 시야

    private EnemyMover mover;
    private Transform player;

    
    
    private void Awake()
    {
        mover = GetComponent<EnemyMover>();                     // 같은 오브젝트에 붙이기
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    public override void Act(Enemy enemy)
    {
        if (mover == null || player == null) return;

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
