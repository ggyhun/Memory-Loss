using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    /// <summary>
    /// Overlay Tilemap에서 적 스폰 위치를 찾아 적을 스폰하는 스크립트입니다.
    /// </summary>

    public GridManager gridManager;
    
    public TileBase enemySpawnTile; // 적 스폰 타일 (Editor에서 할당)
    public GameObject enemyPrefab; // 스폰할 적 프리팹
    
    [Header("Spawn Settings")]
    public int enemyCount = 3; // 스폰할 적의 수
    
    private void Start()
    {
        // GridManager가 Awake에서 맵을 초기화한 후에 적을 스폰합니다.
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        SpawnEnemies();
    }
    
    private void SpawnEnemies()
    {
        // Overlay Tilemap에서 적 스폰위치 중 Eneny 수 만큼 랜덤으로 찾아 적을 스폰합니다.
        BoundsInt bounds = gridManager.overlayTilemap.cellBounds;
        
        List<Vector3Int> spawnPositions = new List<Vector3Int>();

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (gridManager.overlayTilemap.GetTile(pos) == enemySpawnTile)
            {
                spawnPositions.Add(pos);
            }
        }
        System.Random random = new System.Random();
        spawnPositions = spawnPositions.OrderBy(x => random.Next()).ToList();

        // 적 스폰
        int spawnedCount = 0;
        foreach (Vector3Int pos in spawnPositions)
        {
            if (spawnedCount >= enemyCount) break;

            Vector3 worldPosition = gridManager.overlayTilemap.GetCellCenterWorld(pos);
            Instantiate(enemyPrefab, worldPosition, Quaternion.identity);
            spawnedCount++;
            gridManager.SetOccupant(pos, enemyPrefab); // TileData의 occupant를 설정
        }
    }
}
