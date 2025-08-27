using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;
    
    [Header("Other Managers")]
    public GridManager gridManager;
    public EnemyManager enemyManager;

    public TileBase enemySpawnTile; // 적 스폰 타일
    [Header("Fallback (LevelData 없을 때만 사용)")]
    public GameObject defaultEnemyPrefab; // 폴백 프리팹
    public int enemyCount = 3;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        
        if (enemyManager == null)
        {
            enemyManager = FindFirstObjectByType<EnemyManager>();
        }
    }
    
    private void Start()
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
    }

    private void SpawnEnemies(LevelData levelData)
    {
        Debug.Log("EnemySpawner: SpawnEnemies called");
        if (levelData == null)
        {
            Debug.LogWarning("EnemySpawner: levelData is null, using fallback settings.");
        }
        var overlay = gridManager.overlayMap;
        if (overlay == null) { Debug.LogError("EnemySpawner: overlayMap is null."); return; }

        BoundsInt bounds = overlay.cellBounds;
        List<Vector3Int> spawnPositions = new List<Vector3Int>();
        foreach (var pos in bounds.allPositionsWithin)
            if (overlay.GetTile(pos) == enemySpawnTile)
                spawnPositions.Add(pos);

        System.Random random = new System.Random();
        spawnPositions = spawnPositions.OrderBy(_ => random.Next()).ToList();

        int targetCount = levelData.enemyCount;
        int spawned = 0;

        foreach (var cell in spawnPositions)
        {
            if (spawned >= targetCount) break;

            var t = gridManager.GetTileData(cell);
            if (t == null || !t.isWalkable || t.occupant != null)
                continue;

            GameObject prefab = levelData ? levelData.PickEnemyPrefab(random) : defaultEnemyPrefab;
            if (prefab == null) continue;

            Vector3 worldPos = overlay.GetCellCenterWorld(cell);
            // ✅ 맵의 자식으로 붙이기
            GameObject enemy = Instantiate(prefab, worldPos, Quaternion.identity, MapGenerator.Instance.mapInstance.transform);

            gridManager.SetOccupant(cell, enemy);
            spawned++;
        }

        if (spawned < targetCount)
            Debug.LogWarning($"EnemySpawner: 요청 수({targetCount})보다 적게 스폰됨: {spawned}");
    }
    
    public void RespawnEnemies(LevelData ld)
    {
        // GridManager는 이미 MapGenerator가 주입+리빌드를 끝냄
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();

        enemyManager.ClearAllEnemies(); // 기존 적 제거
        // 적 스폰 위치 갱신
        SpawnEnemies(ld);
    }
}
