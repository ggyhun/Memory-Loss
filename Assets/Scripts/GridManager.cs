using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    
    [Header("Tilemaps")]
    public Tilemap backgroundMap;
    public Tilemap obstacleMap;
    public Tilemap overlayMap; // Start 타일이 들어간 Tilemap
    
    [Header("Start Tile")]
    public TileBase startTile; // StartTile Asset (Editor에서 할당)
    
    private Dictionary<Vector3Int, TileData> tileDictionary = new Dictionary<Vector3Int, TileData>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (backgroundMap != null) RebuildTileData();
    }

    public void RebuildTileData()
    {
        tileDictionary.Clear();
        if (!backgroundMap)
        {
            Debug.LogError("GridManager: backgroundMap is null");
            return;
        }

        BoundsInt bounds = backgroundMap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!backgroundMap.HasTile(pos)) continue;

            var data = new TileData {
                cellPosition = pos,
                worldPosition = backgroundMap.GetCellCenterWorld(pos),
                isWalkable = (obstacleMap == null || !obstacleMap.HasTile(pos))
            };
            tileDictionary[pos] = data;
        }
    }
    
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

    private void OnMapChanged(MapContext ctx)
    {
        backgroundMap  = ctx.background;
        obstacleMap    = ctx.obstacle;
        overlayMap     = ctx.overlay; // 없을 수도 있음

        RebuildTileData();
    }

    void InitializeTileData()
    {
        BoundsInt bounds = backgroundMap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin) {
            if (!backgroundMap.HasTile(pos)) continue;

            var data = new TileData {
                cellPosition = pos,
                worldPosition = backgroundMap.GetCellCenterWorld(pos),
                isWalkable = !obstacleMap.HasTile(pos) // 충돌 타일이 없으면 이동 가능
            };

            tileDictionary[pos] = data;
        }
    }

    public TileData GetTileData(Vector3Int pos)
    {
        tileDictionary.TryGetValue(pos, out TileData data);
        return data;
    }

    public TileData GetTileData(Vector3 pos)
    {
        // Vector3를 Vector3Int로 변환하여 TileData를 가져옴
        Vector3Int intPos = backgroundMap.WorldToCell(pos);
        return GetTileData(intPos);
    }

    // 시작 타일의 위치를 찾아 반환
    public Vector3Int FindStartTilePosition()
    {
        if (!overlayMap || !startTile)
        {
            Debug.LogError("overlayTilemap or startTile missing");
            return Vector3Int.zero;
        }

        BoundsInt bounds = overlayMap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (overlayMap.GetTile(pos) == startTile)
                return pos;
        }
        Debug.LogError("Start tile not found!");
        return Vector3Int.zero;
    }
    
    public void SetOccupant(Vector3Int pos, GameObject occupant, bool isWalkable = false)
    {
        if (tileDictionary.TryGetValue(pos, out TileData tileData)) {
            tileData.occupant = occupant;
            tileData.isWalkable = isWalkable;;
        } else {
            Debug.LogWarning($"타일 데이터가 존재하지 않습니다: {pos}");
        }
    }
    
    public void ClearOccupant(Vector3Int pos)
    {
        if (tileDictionary.TryGetValue(pos, out TileData tileData)) {
            tileData.occupant = null;
            tileData.isWalkable = true;
        } else {
            Debug.LogWarning($"타일 데이터가 존재하지 않습니다: {pos}");
        }
    }

    public void MoveTo(GameObject occupant, Vector3 targetPosition)
    {
        Vector3Int startCell = backgroundMap.WorldToCell(occupant.transform.position);
        
        Vector3Int targetCell = backgroundMap.WorldToCell(targetPosition);
        TileData targetTile = GetTileData(targetCell);
        
        if (targetTile != null && targetTile.isWalkable) {
            // 현재 위치에서 타겟 위치로 이동
            occupant.transform.position = targetTile.worldPosition;
            
            // 점유자 정보 업데이트
            ClearOccupant(startCell);
            SetOccupant(targetCell, occupant);
        } else {
            Debug.LogWarning("이동 불가능한 위치입니다: " + targetPosition);
        }
    }

    public void MoveTo(GameObject occupant, Vector3Int targetCell)
    {
        TileData targetTile = GetTileData(targetCell);
        
        if (targetTile != null && targetTile.isWalkable) {
            // 현재 위치에서 타겟 위치로 이동
            occupant.transform.position = targetTile.worldPosition;
            
            // 점유자 정보 업데이트
            Vector3Int startCell = backgroundMap.WorldToCell(occupant.transform.position);
            ClearOccupant(startCell);
            SetOccupant(targetCell, occupant);
        } else {
            Debug.LogWarning("이동 불가능한 위치입니다: " + targetCell);
        }
    }
    
    public List<TileData> GetWalkableNeighbors(Vector3Int cellPos)
    {
        var neighbors = new List<TileData>();
        Vector3Int[] directions = {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        foreach (var dir in directions)
        {
            Vector3Int neighborPos = cellPos + dir;
            if (tileDictionary.TryGetValue(neighborPos, out TileData tile) && tile.isWalkable)
            {
                neighbors.Add(tile);
            }
        }
        return neighbors;
    }
    
    public List<TileData> GetTilesInRange(Vector3Int center, int range)
    {
        var result = new List<TileData>();
        foreach (var pos in tileDictionary.Keys)
        {
            if (Vector3Int.Distance(center, pos) <= range)
            {
                result.Add(tileDictionary[pos]);
            }
        }
        return result;
    }
    
    public GameObject GetOccupant(Vector3Int pos)
    {
        if (tileDictionary.TryGetValue(pos, out TileData tile))
            return tile.occupant;
        return null;
    }
    
    public Vector3Int WorldToCell(Vector3 worldPos) => backgroundMap.WorldToCell(worldPos);
    public Vector3 CellToWorld(Vector3Int cellPos) => backgroundMap.GetCellCenterWorld(cellPos);
}