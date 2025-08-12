using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    [Header("Tilemaps")] public Tilemap backgroundMap;
    public Tilemap obstacleMap;
    public Tilemap overlayMap; // Overlay Tilemap (적 스폰 위치 등)
    
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
        } else {
            Debug.LogWarning($"타일 데이터가 존재하지 않습니다: {pos}");
        }
    }
    
    public void ClearOccupant(Vector3Int pos)
    {
        if (tileDictionary.TryGetValue(pos, out TileData tileData)) {
            tileData.occupant = null;
        } else {
            Debug.LogWarning($"타일 데이터가 존재하지 않습니다: {pos}");
        }
    }
}