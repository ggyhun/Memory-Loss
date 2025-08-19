using UnityEngine;
using UnityEngine.Tilemaps;

public class MapRoot : MonoBehaviour
{
    public Tilemap background;
    public Tilemap obstacle;
    public Tilemap overlay; // 선택(없어도 됨)

    private void Awake()
    {
        // 이름으로 자동 탐색 (비어있을 때만)
        if (!background) background = transform.Find("Background")?.GetComponent<Tilemap>();
        if (!obstacle)   obstacle   = transform.Find("Obstacle")?.GetComponent<Tilemap>();
        if (!overlay)    overlay    = transform.Find("Overlay")?.GetComponent<Tilemap>(); // 없을 수 있음
    }

    public MapContext ToContext()
    {
        return new MapContext
        {
            mapRoot   = gameObject,
            background = background,
            obstacle   = obstacle,
            overlay    = overlay
        };
    }
}
