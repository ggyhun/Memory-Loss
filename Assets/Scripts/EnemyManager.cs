using System.Collections.Generic;
using Unity.Mathematics;
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
            // 적이 없으면 바로 플레이어 턴
            TurnManager.Instance.StartPlayerTurn();
            return;
        }

        foreach (var enemy in enemies)
        {
            enemy.StartTurnAction();
        }
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
}
