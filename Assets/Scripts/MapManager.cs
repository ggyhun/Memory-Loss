using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;
using System.Linq;

public class MapManager : MonoBehaviour
{
    public int FloorSizeX = -1;
    public int FloorSizeY = -1;
    public int[,] FloorData;
    [SerializeField] private TileBase[] tiles;
    public Tilemap tilemap;
    void Start()
    {
        LoadFloorData("Testmap.txt"); // 테스트용 맵 불러오고 적용
        FloorMapUpdate();
    }

    private void LoadFloorData(string name)
    {
        if (!File.Exists(Path.Combine(Application.dataPath, "data\\" + name)))
        {
            Debug.LogError("Missing Floor Data File :" + Path.Combine(Application.dataPath, "data\\" + name));
            return;
        }
        string FileRawData = File.ReadAllText(Path.Combine(Application.dataPath, "data\\" + name));
        string[] tokens = FileRawData.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
        List<int> FileData = new List<int>();
        foreach (string token in tokens) // 리스트로 맵 데이터 변환
        {
            if (int.TryParse(token, out int num))
            {
                FileData.Add(num);
            }
            else
            {
                Debug.LogWarning("Invaild Floor Data File");
            }
        }
        FloorSizeX = FileData[0];
        FloorSizeY = FileData[1];
        int[,] result = new int[FloorSizeY, FloorSizeX];
        //Debug.Log(FileData[1].ToString() + ", " + FileData[0].ToString());
        for (int i = 0; i + 2 < FileData.Count; i++) // 리스트를 2차원 배열로 변환
        {
            result[(i - i % FileData[0]) / FileData[0], i % FileData[0]] = FileData[i + 2];
        }
        FloorData = result;
        return;
    }

    private void FloorMapUpdate()
    {
        for (int y = 0; y < FloorSizeY; y++)
        {
            for (int x = 0; x < FloorSizeX; x++)
            {
                /* Rule Tile 사용 테스트
                if (FloorData[y, x] == 1)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tiles[12]);
                }
                else
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tiles[0]);
                }
                */
                int tiletype = 0;
                int tilerotation = 0;
                if (FloorData[y, x] == 1) // 벽일 경우 타일 적용 - 벽이 아닌 경우는 기본값인 바닥(0)
                {
                    int[,] neighbor = new int[3, 3]; // 맵 경계 벽으로 간주하는 인접 타일 배열
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if ((y == 0 && i == 0) || (y == FloorSizeY - 1 && i == 2) ||
                            (x == 0 && j == 0) || (x == FloorSizeX - 1 && j == 2))
                            {
                                neighbor[i, j] = 1;
                            }
                            else if (FloorData[y + (i - 1), x + (j - 1)] == 1)
                            {
                                neighbor[i, j] = 1;
                            }
                            else
                            {
                                neighbor[i, j] = 0;
                            }
                        }
                    }
                    List<int> cnt1 = new List<int>(); // 상하좌우 인접 타일 확인
                    for (int i = 0; i < 4; i++) // 상단부터 시계방향
                    {
                        if (neighbor[Math.Abs(i - 2), 2 - Math.Abs(i - 1)] == 1)
                        {
                            cnt1.Add(i);
                        }
                    }
                    if (cnt1.Count == 0)
                    {
                        tiletype = 5;
                    }
                    else if (cnt1.Count == 1)
                    {
                        tiletype = 4;
                        tilerotation = cnt1[0] + 3;
                    }
                    else if (cnt1.Count == 2)
                    {
                        if (cnt1[1] - cnt1[0] == 1)
                        {
                            tiletype = 2;
                            tilerotation = cnt1[0];
                        }
                        else if (cnt1[1] - cnt1[0] == 3)
                        {
                            tiletype = 2;
                            tilerotation = 3;
                        }
                        else
                        {
                            tiletype = 3;
                            tilerotation = cnt1[0] + 1;
                        }
                    }
                    else if (cnt1.Count == 3)
                    {
                        tiletype = 1;
                        tilerotation = 1;
                        for (int i = 0; i < cnt1.Count; i++)
                        {
                            if (cnt1[i] != i)
                            {
                                tilerotation = i + 2;
                                break;
                            }
                        }
                    }
                    else if (cnt1.Count == 4)
                    {
                        List<int> cnt2 = new List<int>(); // 대각선 인접 타일 확인
                        for (int i = 0; i < 4; i++) // 좌상단부터 시계방향
                        {
                            if (neighbor[i - i % 2, (i % 3 == 0) ? 0 : 2] == 1)
                            {
                                cnt2.Add(i);
                            }
                        }
                        if (cnt2.Count == 0)
                        {
                            tiletype = 10;
                        }
                        else if (cnt2.Count == 1)
                        {
                            tiletype = 9;
                            tilerotation = cnt2[0] + 3;
                        }
                        else if (cnt2.Count == 2)
                        {
                            if (cnt2[1] - cnt2[0] == 1)
                            {
                                tiletype = 7;
                                tilerotation = 1 - cnt2[0];
                            }
                            else if (cnt2[1] - cnt2[0] == 3)
                            {
                                tiletype = 7;
                                tilerotation = 2;
                            }
                            else
                            {
                                tiletype = 8;
                                tilerotation = cnt2[0] + 1;
                            }
                        }
                        else if (cnt2.Count == 3)
                        {
                            tiletype = 6;
                            tilerotation = -3;
                            for (int i = 0; i < cnt2.Count; i++)
                            {
                                if (cnt2[i] != i)
                                {
                                    tilerotation = -i;
                                    break;
                                }
                            }
                        }
                        else if (cnt2.Count == 4)
                        {
                            tiletype = 11;
                        }
                    }
                }
                tilemap.SetTile(new Vector3Int(x, y, 0), tiles[tiletype]);
                tilemap.SetTransformMatrix(new Vector3Int(x, y, 0), Matrix4x4.Rotate(Quaternion.Euler(0, 0, tilerotation * (-90))));
            }
        }
        return;
    }
    void Update()
    {
        
    }
}
