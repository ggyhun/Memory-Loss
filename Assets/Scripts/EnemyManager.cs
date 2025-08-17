using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private List<Enemy> enemies = new List<Enemy>();
    private int finishedCount = 0;

    public void RegisterEnemy(Enemy enemy) => enemies.Add(enemy);
    public void UnregisterEnemy(Enemy enemy) => enemies.Remove(enemy);

    public void StartEnemyTurn()
    {
        finishedCount = 0;

        if (enemies.Count == 0)
        {
            TurnManager.Instance.StartPlayerTurn();
            return;
        }

        foreach (var enemy in enemies)
            enemy.StartTurnAction();
    }

    public void ReportEnemyDone()
    {
        finishedCount++;
        if (finishedCount >= enemies.Count)
        {
            Debug.Log("모든 적의 턴이 끝났습니다. 플레이어 턴 시작.");
            TurnManager.Instance.StartPlayerTurn();
        }
    }

    public void ClearAllEnemies()
    {
        // 복사본을 돌면서 파괴
        var copy = new List<Enemy>(enemies);
        foreach (var e in copy)
            if (e) Destroy(e.gameObject);

        enemies.Clear();      // ✅ 즉시 비우기
        finishedCount = 0;    // ✅ 카운트 리셋
    }
}