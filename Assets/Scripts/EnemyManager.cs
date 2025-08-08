using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private List<Enemy> enemies = new List<Enemy>();
    private int finishedCount = 0;

    public void RegisterEnemy(Enemy enemy) => enemies.Add(enemy);
    public void UnregisterEnemy(Enemy enemy) => enemies.Remove(enemy);

    public void SpawnEnemy()
    {
        MapManager mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
        List<TileData> EnemySpawnPoint = mapManager.FloorToTileData(4);
        List<TileData> SelectedEnemySpawnPoint = new List<TileData>();
        for (int i = 0; i < mapManager.TotalEnemyCount[mapManager.CurrentFloorLevel]; i++)
        {
            int pick = UnityEngine.Random.Range(0, EnemySpawnPoint.Count);
            SelectedEnemySpawnPoint.Add(EnemySpawnPoint[pick]);
            EnemySpawnPoint.RemoveAt(pick);
        }
        foreach (TileData data in SelectedEnemySpawnPoint)
        {
            // Todo: ? DefaultEnemy와 Enemy의 차이점이 무엇인지 확인 필요
            // Instantiate(DefaultEnemy, new Vector3(data.position.x + 0.5f, data.position.y - 0.5f, data.position.z - 1), new quaternion());
        }
    }
    
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
            TurnManager.Instance.StartPlayerTurn();
        }
    }
}
