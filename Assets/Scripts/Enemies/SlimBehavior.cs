using UnityEngine;

public class SlimBehavior : EnemyBehavior
{
    public override void Act(Enemy enemy)
    {
        Debug.Log($"{enemy.name} 슬라임이 한 칸 다가간다!");
        // TODO: gridManager 등을 이용해서 플레이어 방향으로 이동
    }
}
