using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public static class SpellPatterns
{

    public static List<Vector3Int> GetCastPositions(SpellData data, Vector3Int playerCell)
    {
        var result = new List<Vector3Int>();
        var seen   = new HashSet<Vector3Int>();

        switch (data.castPattern)
        {
            case CastPattern.Around4:
            {
                Vector3Int[] dirs = {
                    Vector3Int.up, 
                    Vector3Int.down, 
                    Vector3Int.left, 
                    Vector3Int.right
                };
                foreach (var d in dirs) AddUnique(playerCell + d);
                break;
            }

            case CastPattern.Single:
            {
                AddUnique(playerCell);
                break;
            }

            case CastPattern.Diagonal:
            {
                Vector3Int[] dirs = { 
                    Vector3Int.up + Vector3Int.left,
                    Vector3Int.up + Vector3Int.right,
                    Vector3Int.down + Vector3Int.right,
                    Vector3Int.down + Vector3Int.left
                };
                foreach (var d in dirs)
                {
                    AddUnique(playerCell + d);
                }
                break;
            }

            case CastPattern.Around4Plus:
            {
                Vector3Int[] dirs =
                {
                    Vector3Int.up * 2,
                    Vector3Int.down * 2,
                    Vector3Int.left * 2,
                    Vector3Int.right * 2,
                };
                foreach (var d in dirs)
                {
                    AddUnique(playerCell + d);
                }
                break;
            }

            case CastPattern.Around8:
            {
                Vector3Int[] dirs =
                {
                    Vector3Int.up,
                    Vector3Int.down,
                    Vector3Int.left,
                    Vector3Int.right,
                    Vector3Int.up + Vector3Int.left,
                    Vector3Int.up + Vector3Int.right,
                    Vector3Int.down + Vector3Int.right,
                    Vector3Int.down + Vector3Int.left
                };
                foreach (var d in dirs)
                {
                    AddUnique(playerCell + d);
                }
                break;
            }
        }
        return result;

        void AddUnique(in Vector3Int cell)
        {
            if (seen.Add(cell)) result.Add(cell);
        }
    }

    /// <summary>
    /// 선택한 캐스트 칸(castCell) 기준 실제 데미지 범위(셀 좌표) 반환.
    /// playerCell은 전방/방향 패턴 계산에 사용.
    /// </summary>
    public static List<Vector3Int> GetAreaPositions(SpellData data, Vector3Int playerCell, Vector3Int castCell)
    {
        var result = new List<Vector3Int>();
        var seen   = new HashSet<Vector3Int>();

        switch (data.areaPattern)
        {
            case AreaPattern.Single:
            {
                AddUnique(castCell);
                break;
            }
            
            case AreaPattern.LineForward2:
            {
                Vector3Int dir = ClampDir(castCell - playerCell);
                if (dir == Vector3Int.zero)
                {
                    AddUnique(castCell);
                    break;
                }

                for (int i = 0; i < 2; i++)
                    AddUnique(castCell + dir * i);
                break;
            }

            case AreaPattern.Around4:
            {
                // 중심 포함, 각 방향으로 n칸
                AddUnique(castCell + Vector3Int.up);
                AddUnique(castCell + Vector3Int.down);
                AddUnique(castCell + Vector3Int.left);
                AddUnique(castCell + Vector3Int.right);
                break;
            }

            case AreaPattern.Circle3:
            {
                // 중심 포함, 원형으로 3칸
                for (int x = -3; x <= 3; x++)
                for (int y = -3; y <= 3; y++)
                {
                    if (x * x + y * y <= 3 * 3) // 원 안에
                    {
                        if (x == 0 && y == 0) continue; // 중심 제외
                        AddUnique(playerCell + new Vector3Int(x, y, 0));
                    }
                }
                break;
            }

            case AreaPattern.Front1:
            { 
                Vector3Int dir = ClampDir(castCell - playerCell);
                if (dir.x == 0)
                {
                    // 수직 방향
                    for (int i = 0; i < 1; i++)
                    {
                        AddUnique(castCell + Vector3Int.left + dir * i);
                        AddUnique(castCell + dir * i);
                        AddUnique(castCell + Vector3Int.right + dir * i);
                    }
                }
                else if (dir.y == 0)
                {
                    // 수평 방향
                    for (int i = 0; i < 1; i++)
                    {
                        AddUnique(castCell + Vector3Int.up + dir * i);
                        AddUnique(castCell + dir * i);
                        AddUnique(castCell + Vector3Int.down + dir * i);
                    }
                }
                break;
            }
            case AreaPattern.Front2:
            {
                Vector3Int dir = ClampDir(castCell - playerCell);
                if (dir.x == 0)
                {
                    // 수직 방향
                    for (int i = 0; i < 2; i++)
                    {
                        AddUnique(castCell + Vector3Int.left + dir * i);
                        AddUnique(castCell + dir * i);
                        AddUnique(castCell + Vector3Int.right + dir * i);
                    }
                }
                else if (dir.y == 0)
                {
                    // 수평 방향
                    for (int i = 0; i < 2; i++)
                    {
                        AddUnique(castCell + Vector3Int.up + dir * i);
                        AddUnique(castCell + dir * i);
                        AddUnique(castCell + Vector3Int.down + dir * i);
                    }
                }
                break;
            }
            case AreaPattern.Front3:
            {
                Vector3Int dir = ClampDir(castCell - playerCell);
                if (dir.x == 0)
                {
                    // 수직 방향
                    for (int i = 0; i < 3; i++)
                    {
                        AddUnique(castCell + Vector3Int.left + dir * i);
                        AddUnique(castCell + dir * i);
                        AddUnique(castCell + Vector3Int.right + dir * i);
                    }
                }
                else if (dir.y == 0)
                {
                    // 수평 방향
                    for (int i = 0; i < 3; i++)
                    {
                        AddUnique(castCell + Vector3Int.up + dir * i);
                        AddUnique(castCell + dir * i);
                        AddUnique(castCell + Vector3Int.down + dir * i);
                    }
                }
                break;
            }
            case AreaPattern.Splash:
            {
                AddUnique(castCell);
                AddUnique(castCell + Vector3Int.up);
                AddUnique(castCell + Vector3Int.down);
                AddUnique(castCell + Vector3Int.left);
                AddUnique(castCell + Vector3Int.right);
                break;
            }
        }
        return result;

        void AddUnique(in Vector3Int cell)
        {
            if (seen.Add(cell)) result.Add(cell);
        }
    }

    /// <summary>
    /// 그리드 방향 정규화. 대각 허용( -1..1 ).
    /// 대각을 금지하려면 ClampDirCardinal 사용.
    /// </summary>
    private static Vector3Int ClampDir(Vector3Int v)
    {
        int x = Mathf.Clamp(v.x, -1, 1);
        int y = Mathf.Clamp(v.y, -1, 1);
        return new Vector3Int(x, y, 0);
    }

    /// <summary>
    /// 상하좌우만 허용하고 싶을 때 사용(필요시 교체).
    /// </summary>
    private static Vector3Int ClampDirCardinal(Vector3Int v)
    {
        int ax = Mathf.Abs(v.x), ay = Mathf.Abs(v.y);
        if (ax >= ay) return new Vector3Int(v.x == 0 ? 0 : (v.x > 0 ? 1 : -1), 0, 0);
        else          return new Vector3Int(0, v.y == 0 ? 0 : (v.y > 0 ? 1 : -1), 0);
    }
}
