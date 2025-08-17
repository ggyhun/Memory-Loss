using UnityEngine;
using System.Collections.Generic;

public static class SpellPatterns
{
    // 플레이어 기준 “시전 가능 칸” 계산
    public static List<Vector3Int> GetCastPositions(SpellData data, Vector3Int playerCell)
    {
        var list = new List<Vector3Int>();
        switch (data.castPattern)
        {
            case CastPattern.Around8:
            {
                Vector3Int[] dirs = {
                    Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right,
                    Vector3Int.up + Vector3Int.left, Vector3Int.up + Vector3Int.right,
                    Vector3Int.down + Vector3Int.left, Vector3Int.down + Vector3Int.right
                };
                foreach (var d in dirs) list.Add(playerCell + d);
                break;
            }
            case CastPattern.Cross4:
            {
                list.Add(playerCell + Vector3Int.up);
                list.Add(playerCell + Vector3Int.down);
                list.Add(playerCell + Vector3Int.left);
                list.Add(playerCell + Vector3Int.right);
                break;
            }
            case CastPattern.Line:
            {
                Vector3Int[] dirs = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
                foreach (var d in dirs)
                {
                    for (int i = 1; i <= Mathf.Max(1, data.castRange); i++)
                        list.Add(playerCell + d * i);
                }
                break;
            }
            case CastPattern.Ring:
            {
                int r = Mathf.Max(1, data.castRange);
                for (int x = -r; x <= r; x++)
                for (int y = -r; y <= r; y++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) == r) // 맨해튼 링
                        list.Add(playerCell + new Vector3Int(x, y, 0));
                }
                break;
            }
            case CastPattern.Custom:
            {
                if (data.castCustomOffsets != null)
                    foreach (var off in data.castCustomOffsets)
                        list.Add(playerCell + (Vector3Int)off);
                break;
            }
        }
        return list;
    }

    // “선택한 캐스트 칸(castCell)” 기준 실제 데미지 범위
    // playerCell을 받는 이유: 방향성 패턴(전방 라인 등)에서 필요
    public static List<Vector3Int> GetAreaPositions(SpellData data, Vector3Int playerCell, Vector3Int castCell)
    {
        var list = new List<Vector3Int>();
        switch (data.areaPattern)
        {
            case AreaPattern.Single:
                list.Add(castCell);
                break;

            case AreaPattern.LineForwardN:
            {
                Vector3Int dir = ClampDir(castCell - playerCell); // 방향 벡터 정규화(그리드)
                int n = Mathf.Max(1, data.areaSize);
                for (int i = 0; i < n; i++)
                    list.Add(castCell + dir * i);
                break;
            }

            case AreaPattern.CrossPlus:
            {
                int n = Mathf.Max(1, data.areaSize);
                for (int i = 0; i <= n; i++)
                {
                    list.Add(castCell + Vector3Int.up    * i);
                    list.Add(castCell + Vector3Int.down  * i);
                    list.Add(castCell + Vector3Int.left  * i);
                    list.Add(castCell + Vector3Int.right * i);
                }
                break;
            }

            case AreaPattern.SquareNxN:
            {
                int n = Mathf.Max(1, data.areaSize);
                int half = n / 2;
                for (int x = -half; x <= half; x++)
                for (int y = -half; y <= half; y++)
                    list.Add(castCell + new Vector3Int(x, y, 0));
                break;
            }

            case AreaPattern.Custom:
            {
                if (data.areaCustomOffsets != null)
                {
                    Vector3Int dir = ClampDir(castCell - playerCell);
                    // 필요하면 dir을 회전 행렬로 적용하는 로직도 추가 가능
                    foreach (var off in data.areaCustomOffsets)
                        list.Add(castCell + (Vector3Int)off);
                }
                break;
            }
        }
        return list;
    }

    private static Vector3Int ClampDir(Vector3Int v)
    {
        int x = Mathf.Clamp(v.x, -1, 1);
        int y = Mathf.Clamp(v.y, -1, 1);
        return new Vector3Int(x, y, 0);
    }
}
