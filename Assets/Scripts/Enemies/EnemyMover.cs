using UnityEngine;

[DisallowMultipleComponent]
public class EnemyMover : MonoBehaviour
{
    [Header("Options")]
    public bool allowDiagonal = false;   // 대각선 한 칸 이동 허용 여부
    public bool avoidExit = true;        // 출구 타일 피할지

    private GridManager grid;

    private void Awake()
    {
        grid = FindFirstObjectByType<GridManager>();
        if (grid == null) Debug.LogError("GridManager not found.");
    }

    // 현재 actor가 target 쪽으로 1칸 이동 시도 (막히면 차선 방향 자동 시도)
    public bool TryStepTowardTarget(GameObject actor, GameObject target)
    {
        if (grid == null || actor == null || target == null) return false;

        Vector3Int actorCell  = grid.WorldToCell(actor.transform.position);
        Vector3Int targetCell = grid.WorldToCell(target.transform.position);
        Vector3Int d = targetCell - actorCell;

        // 주 방향
        Vector3Int step = ComputeStepToward(d, allowDiagonal);
        if (TryMoveTo(actor, actorCell + step)) return true;

        // 차선 방향들
        foreach (var alt in GetAlternatives(d, allowDiagonal))
        {
            if (alt == Vector3Int.zero) continue;
            if (TryMoveTo(actor, actorCell + alt)) return true;
        }
        return false; // 전부 막힘
    }

    // 시야 밖 등에서 상/하/좌/우 중 랜덤 1칸 이동 (불가하면 이동 X)
    public bool TryRandomCardinalStep(GameObject actor)
    {
        if (grid == null || actor == null) return false;

        Vector3Int actorCell = grid.WorldToCell(actor.transform.position);
        Vector3Int[] dirs = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        Shuffle(dirs);

        foreach (var dir in dirs)
        {
            if (TryMoveTo(actor, actorCell + dir)) return true;
        }
        return false;
    }

    // === 내부 유틸 ===
    private bool TryMoveTo(GameObject actor, Vector3Int targetCell)
    {
        var t = grid.GetTileData(targetCell);
        if (t == null) return false;
        if (!t.isWalkable || t.occupant != null) return false;
        if (avoidExit && t.isExit) return false;

        grid.MoveTo(actor, grid.CellToWorld(targetCell)); // 월드 오버로드 사용 권장
        return true;
    }

    private static Vector3Int ComputeStepToward(Vector3Int d, bool allowDiagonal)
    {
        int sx = d.x == 0 ? 0 : (d.x > 0 ? 1 : -1);
        int sy = d.y == 0 ? 0 : (d.y > 0 ? 1 : -1);

        if (allowDiagonal) return new Vector3Int(sx, sy, 0);

        // 4방향: 더 먼 축 우선
        if (Mathf.Abs(d.x) >= Mathf.Abs(d.y))
            return new Vector3Int(sx, 0, 0);
        else
            return new Vector3Int(0, sy, 0);
    }

    private static Vector3Int[] GetAlternatives(Vector3Int d, bool allowDiagonal)
    {
        int sx = d.x == 0 ? 0 : (d.x > 0 ? 1 : -1);
        int sy = d.y == 0 ? 0 : (d.y > 0 ? 1 : -1);

        if (allowDiagonal)
        {
            return new[]
            {
                new Vector3Int(sx, 0, 0),
                new Vector3Int(0, sy, 0),
                new Vector3Int(-sx, 0, 0),
                new Vector3Int(0, -sy, 0)
            };
        }
        else
        {
            if (Mathf.Abs(d.x) >= Mathf.Abs(d.y))
                return new[]
                {
                    new Vector3Int(0, sy, 0),
                    new Vector3Int(0, -sy, 0),
                };
            else
                return new[]
                {
                    new Vector3Int(sx, 0, 0),
                    new Vector3Int(-sx, 0, 0),
                };
        }
    }

    // Fisher–Yates shuffle
    private static void Shuffle<T>(T[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }
}
