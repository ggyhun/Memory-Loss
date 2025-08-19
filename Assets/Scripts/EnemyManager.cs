using System.Collections.Generic;
using UnityEngine;

using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private List<Enemy> enemies = new List<Enemy>();
    private int finishedCount = 0;

    [Header("Floor Progress")]
    [SerializeField] private int killsToAdvance = 5; // ✅ LevelData에서 주입받음 (기본값)
    private int killsThisFloor = 0;

    private void OnEnable()
    {
        if (MapGenerator.Instance != null)
            MapGenerator.Instance.OnMapChanged += OnMapChanged;
    }

    private void OnDisable()
    {
        if (MapGenerator.Instance != null)
            MapGenerator.Instance.OnMapChanged -= OnMapChanged;
    }

    private void OnMapChanged(MapContext _)
    {
        // ✅ 층이 바뀔 때 LevelData에서 목표치 주입 + 카운트 리셋
        var ld = MapGenerator.Instance?.GetCurrentLevelData();
        if (ld != null)
        {
            killsToAdvance = Mathf.Max(1, ld.killsToAdvance);
        }
        killsThisFloor = 0;
        finishedCount  = 0;
        // 필요 시 남은 적 정리: EnemySpawner에서 이미 ClearAllEnemies()를 호출한다면 생략 가능
        // ClearAllEnemies();
        Debug.Log($"[EnemyManager] Floor changed. Goal: {killsToAdvance} kills");
    }

    public void RegisterEnemy(Enemy enemy)   => enemies.Add(enemy);
    public void UnregisterEnemy(Enemy enemy) => enemies.Remove(enemy);

    public void StartEnemyTurn()
    {
        finishedCount = 0;

        var snapshot = enemies.ToArray();
        if (snapshot.Length == 0)
        {
            TurnManager.Instance.StartPlayerTurn();
            return;
        }

        foreach (var enemy in snapshot)
            if (enemy != null) enemy.StartTurnAction();
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

    // ✅ ‘킬’은 여기서만 카운트
    public void ReportEnemyKilled(Enemy enemy)
    {
        killsThisFloor++;
        Debug.Log($"Kill {killsThisFloor}/{killsToAdvance}");

        if (killsThisFloor >= killsToAdvance)
        {
            killsThisFloor = 0;    // 다음 층에서 다시
            // 맵 교체 (Spawn/리빌드는 MapGenerator & Spawners가 처리)
            MapGenerator.Instance.ChangeMap();
        }
    }

    public void ClearAllEnemies()
    {
        var copy = new List<Enemy>(enemies);
        foreach (var e in copy)
            if (e) Destroy(e.gameObject);

        enemies.Clear();
        finishedCount = 0;
    }
}
