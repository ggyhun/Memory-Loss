using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class HighlightManager : MonoBehaviour
{
    public GameObject highlightPrefab;     // SpriteRenderer 프리팹
    public Tilemap tilemap;
    public GridManager gridManager;

    public int poolSize = 8;

    private List<GameObject> pool = new List<GameObject>();
    private List<GameObject> activeHighlights = new List<GameObject>();

    private Vector3Int[] directions = {
        Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
    };

    void Awake()
    {
        InitializePool();
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = Instantiate(highlightPrefab, Vector3.zero, Quaternion.identity, transform);
            go.SetActive(false);
            pool.Add(go);
        }
    }

    GameObject GetFromPool()
    {
        foreach (var go in pool)
        {
            if (!go.activeInHierarchy)
                return go;
        }

        GameObject newGo = Instantiate(highlightPrefab, Vector3.zero, Quaternion.identity, transform);
        newGo.SetActive(false);
        pool.Add(newGo);
        return newGo;
    }

    public void ShowHighlights(Vector3Int playerPos, System.Action<Vector3Int> onClick)
    {
        ClearHighlights();

        foreach (var dir in directions)
        {
            Vector3Int target = playerPos + dir;
            TileData data = gridManager.GetTileData(target);

            if (data != null && data.isWalkable && data.occupant == null)
            {
                GameObject go = GetFromPool();
                go.transform.position = tilemap.GetCellCenterWorld(target);
                go.SetActive(true);

                HighlightTile tile = go.GetComponent<HighlightTile>();
                tile.Setup(target, onClick);

                activeHighlights.Add(go);
            }
        }
    }

    public void ClearHighlights()
    {
        foreach (var go in activeHighlights)
        {
            go.SetActive(false);
        }
        activeHighlights.Clear();
    }
}
