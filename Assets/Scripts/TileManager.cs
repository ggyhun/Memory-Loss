using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


/*
public class TileManager : MonoBehaviour
{
    public Tilemap backgroundMap;
    public Tilemap obstacleMap;

    private Dictionary<Vector3Int, TileData> tileDictionary = new Dictionary<Vector3Int, TileData>();

    void Awake()
    {
        InitializeTileData();
    }
    

    void InitializeTileData()
    {
        BoundsInt bounds = backgroundMap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!backgroundMap.HasTile(pos)) continue;

            var data = new TileData
            {
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
    
    public Tilemap markerTilemap; // Start 타일이 들어간 Tilemap
    public TileBase startTile;    // StartTile Asset (Editor에서 할당)

    
    // 시작 타일의 위치를 찾아 반환
    public Vector3Int FindStartTilePosition()
    {
        BoundsInt bounds = markerTilemap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (markerTilemap.GetTile(pos) == startTile)
            {
                return pos;
            }
        }
        Debug.LogError("시작 지점을 찾을 수 없습니다!");
        return Vector3Int.zero;
    }
    
    // 타일의 점유자를 설정하거나 제거하는 메서드
    
    public void SetOccupantAt(Vector3Int pos, GameObject unit)
    {
        if (tileDictionary.TryGetValue(pos, out var tileData))
            tileData.occupant = unit;
    }

    public void ClearOccupantAt(Vector3Int pos)
    {
        if (tileDictionary.TryGetValue(pos, out var tileData))
            tileData.occupant = null;
    }
}
*/
