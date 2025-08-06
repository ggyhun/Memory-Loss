using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System;

public class EnemyManager : MonoBehaviour
{
    public GameObject DefaultEnemy;
    public List<GameObject> EnemyList = new List<GameObject>();
    void Start()
    {
        
    }

    public void SpawnEnemy()
    {
        MapManager mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
        List<TileData> EnemySpawnPoint = mapManager.FloorToTileData(4);
        List<TileData> SelectedEnemySpawnPoint = new List<TileData>();
        for (int i = 0; i < mapManager.TotalEnemyCount[mapManager.CurrentFloorLevel]; i++)
        {
            int pick = UnityEngine.Random.Range(0, EnemySpawnPoint.Count);
            SelectedEnemySpawnPoint.Add(EnemySpawnPoint[pick]);
            EnemySpawnPoint.RemoveAt(pick);
        }
        foreach (TileData data in SelectedEnemySpawnPoint)
        {
            Instantiate(DefaultEnemy, new Vector3(data.position.x + 0.5f, data.position.y - 0.5f, data.position.z - 1), new quaternion());
        }
    }

    public void RegisterEnemy(GameObject Enemy)
    {
        EnemyList.Add(Enemy);
    }

    public void UnregisterEnemy(GameObject Enemy)
    {
        EnemyList.Remove(Enemy);
    }

    void Update()
    {
        
    }
}
