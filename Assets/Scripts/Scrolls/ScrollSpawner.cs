using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class ScrollSpawner : MonoBehaviour
{
    /// <summary>
    /// Overlay Tilemap에서 스크롤 스폰 위치를 찾아 주문서를 스폰합니다.
    /// </summary>
    public GridManager gridManager;

    [Header("Spawn Sources")]
    public TileBase scrollSpawnTile;     // 스크롤 스폰 타일
    public GameObject scrollPrefab;      // 스크롤 프리팹 (Scroll 컴포넌트 포함)

    [Header("Fallback (LevelData 없을 때만 사용)")]
    public int scrollCount = 3;
    public bool assignSpellFromPool = true;
    public List<SpellData> spellPool = new List<SpellData>();

    [Header("Level Data")]
    public LevelData levelData;

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

    private void Start()
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        SpawnScrolls();
    }

    private void SpawnScrolls()
    {
        // (맵의 자식으로 스폰하면 ClearAllScrolls 생략 가능)
        // ClearAllScrolls();

        levelData = MapGenerator.Instance.GetCurrentLevelData();

        if (gridManager == null || gridManager.overlayMap == null) { Debug.LogError("ScrollSpawner: Grid/overlayMap null."); return; }
        if (scrollPrefab == null) { Debug.LogError("ScrollSpawner: scrollPrefab is null."); return; }

        var overlay = gridManager.overlayMap;
        BoundsInt bounds = overlay.cellBounds;
        List<Vector3Int> spawnCells = new List<Vector3Int>();

        foreach (var pos in bounds.allPositionsWithin)
            if (overlay.GetTile(pos) == scrollSpawnTile)
                spawnCells.Add(pos);

        System.Random rnd = new System.Random();
        spawnCells = spawnCells.OrderBy(_ => rnd.Next()).ToList();

        int targetCount = levelData ? levelData.scrollCount : scrollCount;
        int spawned = 0;

        foreach (var cell in spawnCells)
        {
            if (spawned >= targetCount) break;

            var t = gridManager.GetTileData(cell);
            if (t == null || !t.isWalkable || t.occupant != null)
                continue;

            Vector3 worldPos = overlay.GetCellCenterWorld(cell);
            // ✅ 맵의 자식으로 붙이기
            GameObject instance = Instantiate(scrollPrefab, worldPos, Quaternion.identity, MapGenerator.Instance.mapInstance.transform);

            // 주문서이기에 isWalkable을 true로 설정
            gridManager.SetOccupant(cell, instance, true);

            var scroll = instance.GetComponent<Scroll>();
            if (scroll != null)
            {
                SpellData picked = null;
                if (levelData != null) picked = levelData.PickScrollSpell(rnd);
                else if (assignSpellFromPool && spellPool.Count > 0) picked = spellPool[rnd.Next(spellPool.Count)];
                scroll.spellData = picked;
            }

            spawned++;
        }

        if (spawned < targetCount)
            Debug.LogWarning($"ScrollSpawner: 요청 수({targetCount})보다 적게 스폰됨: {spawned}");
    }
    
    private void OnMapChanged(MapContext context)
    {
        gridManager = FindFirstObjectByType<GridManager>();
        
        levelData = MapGenerator.Instance.GetCurrentLevelData();
        SpawnScrolls();
    }

    private void ClearAllScrolls()
    {
        if (gridManager == null || gridManager.overlayMap == null) return;

        var overlay = gridManager.overlayMap;
        BoundsInt bounds = overlay.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (overlay.GetTile(pos) == scrollSpawnTile)
            {
                var tileData = gridManager.GetTileData(pos);
                if (tileData != null && tileData.occupant != null)
                {
                    Destroy(tileData.occupant);
                    gridManager.ClearOccupant(pos);   // ✅ SetOccupant(pos, null) → ClearOccupant(pos)
                }
            }
        }
    }
}
