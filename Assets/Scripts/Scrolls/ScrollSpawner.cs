using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ScrollSpawner : MonoBehaviour
{
    [Header("Other Managers")]
    public GridManager gridManager;
    public MapGenerator mapGenerator;

    [Header("Spawn Markers")]
    public TileBase scrollSpawnTile;

    [Header("Fallback (LevelData 없을 때만)")]
    public GameObject fallbackScrollPrefab;     // LevelData 없거나 비었을 때 쓸 기본 프리팹
    public int scrollCount = 3;
    public bool assignSpellFromPool = true;
    public List<SpellData> spellPool = new List<SpellData>();

    [Header("Level Data")]
    public LevelData levelData;
    
    private void OnEnable()
    {
        mapGenerator.OnMapChanged += OnMapChanged;
    }

    private void OnDisable()
    {
        mapGenerator.OnMapChanged -= OnMapChanged;
    }

    private void Start()
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
    }

    private void SpawnScrolls()
    {
        levelData = MapGenerator.Instance.GetCurrentLevelData();

        if (gridManager == null || gridManager.overlayMap == null)
        { Debug.LogError("ScrollSpawner: Grid/overlayMap null."); return; }

        var overlay = gridManager.overlayMap;
        var bounds  = overlay.cellBounds;

        var spawnCells = new List<Vector3Int>();
        foreach (var pos in bounds.allPositionsWithin)
            if (overlay.GetTile(pos) == scrollSpawnTile)
                spawnCells.Add(pos);

        var rnd = new System.Random();
        spawnCells = spawnCells.OrderBy(_ => rnd.Next()).ToList();

        int targetCount = (levelData != null) ? levelData.scrollCount : scrollCount;
        int spawned = 0;

        foreach (var cell in spawnCells)
        {
            if (spawned >= targetCount) break;

            var t = gridManager.GetTileData(cell);
            if (t == null || !t.isWalkable || t.occupant != null) continue;

            // 1) 프리팹 선택: LevelData 풀 → 폴백 프리팹
            GameObject prefab = (levelData != null) ? levelData.PickScrollPrefab(rnd) : null;
            if (prefab == null) prefab = fallbackScrollPrefab;
            if (prefab == null) { Debug.LogWarning("ScrollSpawner: no prefab to spawn."); break; }

            Vector3 worldPos = overlay.GetCellCenterWorld(cell);
            var instance = Instantiate(prefab, worldPos, Quaternion.identity, MapGenerator.Instance.mapInstance.transform);

            // 스크롤은 밟을 수 있게 하고 싶다면, 점유자 로직은 프로젝트 정책에 맞춰 처리
            gridManager.SetOccupant(cell, instance, true);

            // 2) 스펠 보정: 프리팹의 Scroll.spellData가 비어있으면 LevelData 스펠 풀(가중치)로 보충
            var scroll = instance.GetComponent<Scroll>();
            if (scroll != null && scroll.spellData == null)
            {
                SpellData picked = null;
                if (levelData != null) picked = levelData.PickScrollSpell(rnd);
                else if (assignSpellFromPool && spellPool.Count > 0) picked = spellPool[rnd.Next(spellPool.Count)];

                if (picked != null)
                {
                    scroll.spellData = picked;
                    var sr = instance.GetComponent<SpriteRenderer>();
                    if (sr != null && picked.icon != null) sr.sprite = picked.icon;
                }
            }

            spawned++;
        }

        if (spawned < targetCount)
            Debug.LogWarning($"ScrollSpawner: 요청 수({targetCount})보다 적게 스폰됨: {spawned}");
    }

    private void OnMapChanged(MapContext context)
    {
        gridManager = FindFirstObjectByType<GridManager>();
        levelData   = MapGenerator.Instance.GetCurrentLevelData();
        SpawnScrolls();
    }
}
