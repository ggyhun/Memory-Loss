using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyMover : MonoBehaviour
{
    [Header("Options")]
    public bool allowDiagonal = false;   // 대각선 한 칸 이동 허용
    public bool avoidExit = true;        // 출구 타일 회피
    public bool useClaims = true;        // GridManager의 셀 클레임 사용(동시 이동 충돌 방지)

    [Header("Visual Move")]
    public EnemyAnimator enemyAnimator;
    public float fakeStepDuration = 0.2f;                 // 시각 이동 시간
    public AnimationCurve ease = AnimationCurve.EaseInOut(0,0,1,1);
    public Transform defaultVisualRoot;                    // 비워두면 자동으로 찾아줌(자식 Sprite)
    
    private GridManager grid;

    private void Awake()
    {
        grid = FindFirstObjectByType<GridManager>();
        if (!grid) Debug.LogError("GridManager not found.");
    }

    // =================== 외부에서 호출하는 API ===================

    /// <summary>타깃 쪽으로 1칸 이동 시도(막히면 차선 방향 시도)</summary>
    public bool TryStepTowardTarget(GameObject actor, GameObject target)
    {
        if (!grid || !actor || !target) return false;

        var actorCell  = grid.WorldToCell(actor.transform.position);
        var targetCell = grid.WorldToCell(target.transform.position);
        var d = targetCell - actorCell;

        // 주 방향
        var step = ComputeStepToward(d, allowDiagonal);
        if (TryMoveTo(actor, actorCell + step))
        {
            enemyAnimator.PlayMove(step);
            return true;
        }

        // 차선 방향들
        foreach (var alt in GetAlternatives(d, allowDiagonal))
        {
            if (alt == Vector3Int.zero) continue;
            if (TryMoveTo(actor, actorCell + alt))
            {
                enemyAnimator.PlayMove(alt);
                return true;
            }
        }
        return false;
    }

    /// <summary>시야 밖 등에서 상/하/좌/우 중 랜덤 1칸 이동(불가하면 X)</summary>
    public bool TryRandomCardinalStep(GameObject actor)
    {
        if (!grid || !actor) return false;

        var actorCell = grid.WorldToCell(actor.transform.position);
        Vector3Int[] dirs = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        Shuffle(dirs);

        foreach (var dir in dirs)
            if (TryMoveTo(actor, actorCell + dir))
            {
                enemyAnimator.PlayMove(dir);
                return true;
            }

        return false;
    }

    /// <summary>지정 인덱스 방향으로 한 칸(0:위, 1:아래, 2:왼, 3:오)</summary>
    public bool TryMoveToward(GameObject actor, int direction)
    {
        if (!grid || !actor) return false;

        var actorCell = grid.WorldToCell(actor.transform.position);
        Vector3Int[] dirs = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        direction = Mathf.Clamp(direction, 0, dirs.Length - 1);
        return TryMoveTo(actor, actorCell + dirs[direction]);
    }

    // =================== 내부 구현 ===================

    private bool TryMoveTo(GameObject actor, Vector3Int targetCell)
    {
        // 시각 이동 중이면 중복 명령 방지
        var visual = EnsureVisual(actor);
        if (visual.IsMoving) return false;

        // 타일 유효성
        var t = grid.GetTileData(targetCell);
        if (t == null) return false;
        if (!t.isWalkable || t.occupant != null) return false;
        if (avoidExit && t.isExit) return false;

        // 클레임(선점)
        if (useClaims && !grid.TryClaimCell(targetCell)) return false;

        // 시각 이동 + 스냅
        StartCoroutine(Co_Move(actor, visual, targetCell));
        Debug.Log($"EnemyMover: Moved to {targetCell}");
        return true;
    }

    private IEnumerator Co_Move(GameObject actor, GridMoveVisual visual, Vector3Int targetCell)
    {
        // 시각 이동 파라미터 설정
        visual.fakeDuration = fakeStepDuration;
        visual.ease = ease;

        // visualRoot 자동 설정(없다면 가장 가까운 SpriteRenderer)
        if (visual.visualRoot == null)
        {
            var sr = actor.GetComponentInChildren<SpriteRenderer>();
            visual.visualRoot = sr ? sr.transform : actor.transform; // 최후: 자기 자신
        }

        // 시각 이동(루트는 제자리 + local offset), 마지막에 GridManager.MoveTo로 스냅
        yield return visual.FakeThenSnap(grid, targetCell, useClaim: useClaims);
    }

    // ----------------- 경로 계산 유틸 -----------------

    private static Vector3Int ComputeStepToward(Vector3Int d, bool allowDiag)
    {
        int sx = d.x == 0 ? 0 : (d.x > 0 ? 1 : -1);
        int sy = d.y == 0 ? 0 : (d.y > 0 ? 1 : -1);

        if (allowDiag) return new Vector3Int(sx, sy, 0);

        // 4방향: 더 먼 축 우선
        if (Mathf.Abs(d.x) >= Mathf.Abs(d.y))
            return new Vector3Int(sx, 0, 0);
        else
            return new Vector3Int(0, sy, 0);
    }

    private static Vector3Int[] GetAlternatives(Vector3Int d, bool allowDiag)
    {
        int sx = d.x == 0 ? 0 : (d.x > 0 ? 1 : -1);
        int sy = d.y == 0 ? 0 : (d.y > 0 ? 1 : -1);

        if (allowDiag)
        {
            return new[]
            {
                new Vector3Int(sx, 0, 0),
                new Vector3Int(0, sy, 0),
                new Vector3Int(-sx, 0, 0),
                new Vector3Int(0, -sy, 0),
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

    // ----------------- 공용 유틸 -----------------

    private static void Shuffle<T>(T[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }

    private GridMoveVisual EnsureVisual(GameObject actor)
    {
        var v = actor.GetComponent<GridMoveVisual>();
        if (!v) v = actor.AddComponent<GridMoveVisual>();
        // 기본 visualRoot 지정이 있으면 반영
        if (!v.visualRoot && defaultVisualRoot) v.visualRoot = defaultVisualRoot;
        return v;
    }
}
