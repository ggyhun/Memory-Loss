using System.Collections;
using UnityEngine;

public class GridMoveVisual : MonoBehaviour
{
    [Header("Visual Only")]
    public Transform visualRoot;          // 스프라이트 붙은 자식(없으면 this.transform)
    public float fakeDuration = 0.25f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0,0,1,1);

    public bool IsMoving { get; private set; }

    public IEnumerator FakeThenSnap(GridManager grid, Vector3Int targetCell, bool useClaim = false)
    {
        if (IsMoving) yield break;
        if (visualRoot == null) visualRoot = transform;

        if (useClaim) {
            if (!grid.TryClaimCell(targetCell)) yield break;
        } else {
            if (!grid.IsFreeConsideringClaims(targetCell)) yield break;
        }

        IsMoving = true;

        Vector3Int fromCell = grid.WorldToCell(transform.position);
        Vector3 fromWorld   = grid.CellToWorld(fromCell);
        Vector3 toWorld     = grid.CellToWorld(targetCell);
        Vector3 deltaWorld  = toWorld - fromWorld;

        Vector3 startLocal  = visualRoot.localPosition;
        float t = 0f, dur = Mathf.Max(0.0001f, fakeDuration);

        while (t < 1f) {
            t += Time.deltaTime / dur;
            float k = ease.Evaluate(Mathf.Clamp01(t));
            visualRoot.localPosition = startLocal + deltaWorld * k;
            yield return null;
        }

        // 시각 루트 원위치
        visualRoot.localPosition = startLocal;

        // 실제 좌표/점유는 마지막에 한 번!
        grid.MoveTo(gameObject, targetCell);

        if (useClaim) grid.ReleaseClaimCell(targetCell);
        IsMoving = false;
    }
}
