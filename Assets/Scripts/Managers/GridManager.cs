using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap backgroundMap;
    public Tilemap obstacleMap;
    public Tilemap overlayMap;
    
    private Dictionary<Vector3Int, TileData> tileDictionary = new Dictionary<Vector3Int, TileData>();

    void Awake()
    {
        InitializeTileData();
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

    public Tilemap overlayTilemap; // Start 타일이 들어간 Tilemap
    public TileBase startTile; // StartTile Asset (Editor에서 할당)

    // 시작 타일의 위치를 찾아 반환
    public Vector3Int FindStartTilePosition()
    {
        BoundsInt bounds = overlayTilemap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin) {
            if (overlayTilemap.GetTile(pos) == startTile) {
                return pos;
            }
        }

        Debug.LogError("시작 지점을 찾을 수 없습니다!");
        return Vector3Int.zero;
    }
    
    public void SetOccupant(Vector3Int pos, GameObject occupant)
    {
        if (tileDictionary.TryGetValue(pos, out TileData tileData)) {
            tileData.occupant = occupant;
            tileData.isWalkable = false;
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
        
        if (targetTile != null && targetTile.isWalkable && targetTile.occupant == null) {
            // 현재 위치에서 타겟 위치로 이동
            occupant.transform.position = targetTile.worldPosition;
            
            // 점유자 정보 업데이트
            ClearOccupant(startCell);
            SetOccupant(targetCell, occupant);
        } else {
            Debug.LogWarning("이동 불가능한 위치입니다: " + targetPosition);
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
            if (tileDictionary.TryGetValue(neighborPos, out TileData tile) && tile.isWalkable && tile.occupant == null)
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