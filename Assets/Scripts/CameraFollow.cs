using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                  // 비워두면 "Player" 태그를 자동 탐색
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Smoothing")]
    [Range(0f, 0.5f)] public float smoothTime = 0.15f; // 0이면 즉시 스냅
    public bool snapOnStart = true;

    [Header("Bounds")]
    public Tilemap boundsTilemap;             // 비워두면 MapGenerator 이벤트로 주입
    private bool hasBounds;
    private Vector2 minClamp, maxClamp;

    private Camera cam;
    private Vector3 velocity;

    private void Awake()
    {
        GameRecorder.Instance.ResetAllRecorded();
        cam = GetComponent<Camera>();
    }
    
    private void OnEnable()
    {
        if (MapGenerator.Instance != null)
            MapGenerator.Instance.OnMapChanged += OnMapChanged;
    }

    private void OnDisable()
    {
        if (MapGenerator.Instance != null)
            MapGenerator.Instance.OnMapChanged -= OnMapChanged;
    }

    private void Start()
    {
        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
        }
        if (boundsTilemap) ComputeBounds(boundsTilemap);
        if (snapOnStart && target) transform.position = ClampPos(target.position + offset);
    }

    private void LateUpdate()
    {
        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (!p) return;
            target = p.transform;
        }

        Vector3 desired = ClampPos(target.position + offset);
        transform.position = (smoothTime <= 0f)
            ? desired
            : Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }

    private void OnMapChanged(MapContext ctx)
    {
        boundsTilemap = ctx.background;        // 배경 타일맵을 경계로 사용
        ComputeBounds(boundsTilemap);

        if (target)                            // 맵 바뀔 때 한 번 스냅
            transform.position = ClampPos(target.position + offset);
    }

    private void ComputeBounds(Tilemap t)
    {
        if (!t) { hasBounds = false; return; }

        var cb = t.cellBounds;

        // 월드 코너 계산 (셀 중심 ± half cell)
        var cell = t.cellSize;
        var minCenter = t.GetCellCenterWorld(cb.min);
        var maxCenter = t.GetCellCenterWorld(cb.max - new Vector3Int(1, 1, 0));
        var worldMin = new Vector2(minCenter.x - cell.x * 0.5f, minCenter.y - cell.y * 0.5f);
        var worldMax = new Vector2(maxCenter.x + cell.x * 0.5f, maxCenter.y + cell.y * 0.5f);

        // 카메라 절두체 반경만큼 안쪽으로 클램프
        float vert = cam.orthographicSize;
        float horz = vert * cam.aspect;
        minClamp = new Vector2(worldMin.x + horz, worldMin.y + vert);
        maxClamp = new Vector2(worldMax.x - horz, worldMax.y - vert);

        // 맵이 카메라보다 작은 경우를 대비해 보정
        if (minClamp.x > maxClamp.x) { float m = (minClamp.x + maxClamp.x) * 0.5f; minClamp.x = maxClamp.x = m; }
        if (minClamp.y > maxClamp.y) { float m = (minClamp.y + maxClamp.y) * 0.5f; minClamp.y = maxClamp.y = m; }

        hasBounds = true;
    }

    public IEnumerator InGameZoomInOut(float StartSize = 5, float EndSize = 5, float t = 3)
    {
        cam.orthographicSize = StartSize;
        float timecount = 0;
        while (timecount <= t)
        {
            cam.orthographicSize = StartSize - (StartSize - EndSize) * timecount / t;
            timecount += Time.deltaTime;
            yield return null;
        }
        cam.orthographicSize = EndSize;
        yield break;
    }

    private Vector3 ClampPos(Vector3 pos)
    {
        if (!hasBounds) return pos;
        float x = Mathf.Clamp(pos.x, minClamp.x, maxClamp.x);
        float y = Mathf.Clamp(pos.y, minClamp.y, maxClamp.y);
        return new Vector3(x, y, pos.z);
    }
}
