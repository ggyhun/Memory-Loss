using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct MapContext
{
    public GameObject mapRoot;
    public Tilemap background;
    public Tilemap obstacle;
    public Tilemap overlay; // 있을 수도, 없을 수도
}

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance;
    public event Action<MapContext> OnMapChanged;

    public List<GameObject> mapPrefabs;
    public GameObject BossMapPrefabs;
    public GameObject mapInstance;

    [Header("Level Data")]
    public List<LevelData> levelDataList;
    int currentLevelIndex = 0;  // 0-based로 권장
    
    [Header("GridManager Prefab")]
    public GameObject gridManagerPrefab;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (mapPrefabs == null || mapPrefabs.Count == 0)
        {
            Debug.LogError("Map prefabs are not assigned in the MapGenerator.");
            return;
        }
        // Awake 단계에서 다른 객체가 구독이 안되어있어 주석처리
        // StartCoroutine(ChangeMap());
    }
    
    private void Start()
    {
        if (mapPrefabs == null || mapPrefabs.Count == 0)
        {
            Debug.LogError("Map prefabs are not assigned in the MapGenerator.");
            return;
        }

        // ✅ 시작 시 맵 생성
        StartCoroutine(ChangeMap());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            StartCoroutine(ChangeMap());
    }

    public IEnumerator ChangeMap()
    {
        if (GameRecorder.Instance.GetReachedFloor() >= 10)
        {
            StartCoroutine(GameManager.Instance.GameClear());
        } else
        {
            if (GameRecorder.Instance.GetReachedFloor() != 0)
                yield return FadeManager.Instance.FadeInOut(1, 1.5f);

            if (GameRecorder.Instance.GetReachedFloor() <= 3)
                AudioManager.Instance.PlayBGM(1);
            else if (GameRecorder.Instance.GetReachedFloor() <= 6)
                AudioManager.Instance.PlayBGM(2);
            else if (GameRecorder.Instance.GetReachedFloor() <= 9)
                AudioManager.Instance.PlayBGM(3);

            if (GameRecorder.Instance.GetReachedFloor() == 9)
            {
                if (mapInstance) Destroy(mapInstance);
                Debug.Log("MapGenerator: Changing map...");
                SpawnBossMap();
                GameRecorder.Instance.AddReachedFloor();
            }
            else
            {
                if (mapInstance) Destroy(mapInstance);
                Debug.Log("MapGenerator: Changing map...");
                SpawnRandomMap();
                GameRecorder.Instance.AddReachedFloor();
            }
            InGameUiManager.Instance.currentFloor = GameRecorder.Instance.GetReachedFloor();
        }
        yield break;
    }

    private void SpawnRandomMap()
    {
        var prefab = mapPrefabs[UnityEngine.Random.Range(0, mapPrefabs.Count)];
        mapInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        var background = mapInstance.transform.Find("Background")?.GetComponent<Tilemap>();
        var decorative = mapInstance.transform.Find("Decorative")?.GetComponent<Tilemap>();
        var obstacle = mapInstance.transform.Find("Obstacle")?.GetComponent<Tilemap>();
        var overlay = mapInstance.transform.Find("Overlay")?.GetComponent<Tilemap>();

        if (!background) Debug.LogError("[MapGenerator] Background Tilemap not found on new map.");
        if (!obstacle) Debug.LogWarning("[MapGenerator] Obstacle Tilemap not found (optional).");

        var grid = FindFirstObjectByType<GridManager>();
        if (grid == null && gridManagerPrefab != null)
            grid = Instantiate(gridManagerPrefab, Vector3.zero, Quaternion.identity).GetComponent<GridManager>();

        if (grid != null)
        {
            grid.backgroundMap = background;
            grid.obstacleMap = obstacle;
            grid.overlayMap = overlay;
            grid.RebuildTileData(); // ✅ 먼저 빌드
        }
        else
        {
            Debug.LogError("[MapGenerator] GridManager not found in scene.");
        }

        var ctx = new MapContext { mapRoot = mapInstance, background = background, obstacle = obstacle, overlay = overlay };

        // ✅ 현재 레벨 데이터로 스폰들이 동작하도록 먼저 브로드캐스트
        OnMapChanged?.Invoke(ctx);
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        pc.OnMapChanged(); // 플레이어 컨트롤러에게도 맵 변경 알림

        // ✅ 이벤트가 끝난 뒤 다음 레벨로 증가(리스트 범위 내에서)
        if (levelDataList != null && levelDataList.Count > 0)
            currentLevelIndex = Mathf.Min(currentLevelIndex + 1, levelDataList.Count - 1);
    }

    private void SpawnBossMap()
    {
        var prefab = BossMapPrefabs;
        mapInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        var background = mapInstance.transform.Find("Background")?.GetComponent<Tilemap>();
        var decorative = mapInstance.transform.Find("Decorative")?.GetComponent<Tilemap>();
        var obstacle = mapInstance.transform.Find("Obstacle")?.GetComponent<Tilemap>();
        var overlay = mapInstance.transform.Find("Overlay")?.GetComponent<Tilemap>();

        if (!background) Debug.LogError("[MapGenerator] Background Tilemap not found on new map.");
        if (!obstacle) Debug.LogWarning("[MapGenerator] Obstacle Tilemap not found (optional).");

        var grid = FindFirstObjectByType<GridManager>();
        if (grid == null && gridManagerPrefab != null)
            grid = Instantiate(gridManagerPrefab, Vector3.zero, Quaternion.identity).GetComponent<GridManager>();

        if (grid != null)
        {
            grid.backgroundMap = background;
            grid.obstacleMap = obstacle;
            grid.overlayMap = overlay;
            grid.RebuildTileData(); // ✅ 먼저 빌드
        }
        else
        {
            Debug.LogError("[MapGenerator] GridManager not found in scene.");
        }

        var ctx = new MapContext { mapRoot = mapInstance, background = background, obstacle = obstacle, overlay = overlay };

        // ✅ 현재 레벨 데이터로 스폰들이 동작하도록 먼저 브로드캐스트
        OnMapChanged?.Invoke(ctx);
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        pc.OnMapChanged(); // 플레이어 컨트롤러에게도 맵 변경 알림

        // ✅ 이벤트가 끝난 뒤 다음 레벨로 증가(리스트 범위 내에서)
        if (levelDataList != null && levelDataList.Count > 0)
            currentLevelIndex = Mathf.Min(currentLevelIndex + 1, levelDataList.Count - 1);
    }
    
    public LevelData GetCurrentLevelData()
    {
        if (levelDataList == null || levelDataList.Count == 0) return null;
        currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, levelDataList.Count - 1);
        return levelDataList[currentLevelIndex];
    }
}