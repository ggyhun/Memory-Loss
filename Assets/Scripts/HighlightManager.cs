using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class HighlightManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject castHighlighterPrefab;
    public GameObject spellHighlighterPrefab;
    public GameObject moveHighlighterPrefab;

    // ✅ 애니 관련 옵션 (인스펙터에서 상태 이름/레이어 확인)
    [Header("Animation Wait")]
    public string attackStateName = "Attack"; // Animator의 공격 State 이름
    public int attackLayer = 0;               // 레이어 인덱스
    public float attackEndTimeout = 3f;       // 안전 타임아웃(초)
    
    [Header("Other Managers")]
    public MapGenerator mapGenerator;
    public GridManager gridManager;
    public TurnManager turnManager;


    private List<CastHighlighter> castHighlighters = new List<CastHighlighter>();
    private List<SpellHighlighter> spellHighlighters = new List<SpellHighlighter>();
    private List<GameObject> moveHighlighters = new List<GameObject>();

    private SpellInstance currentSpell;
    private GameObject player;

    public void Init(GameObject player, GridManager grid)
    {
        this.player = player;
        this.gridManager = grid;
    }
    
    private void Awake()
    {
        if (player == null) player = FindFirstObjectByType<PlayerController>().gameObject;
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (turnManager == null) turnManager = FindFirstObjectByType<TurnManager>();
        if (mapGenerator == null) mapGenerator = FindFirstObjectByType<MapGenerator>();

        for (int i = 0; i < 4; i++)
        {
            GameObject moveHighlighter = Instantiate(moveHighlighterPrefab);
            moveHighlighter.GetComponent<MoveHighlighter>().highlightManager = this;
            moveHighlighter.SetActive(false);
            moveHighlighters.Add(moveHighlighter);
        }
    }
    
    private void OnEnable()
    {
        mapGenerator.OnMapChanged += OnMapChanged;
    }
    
    private void OnDisable()
    {
        mapGenerator.OnMapChanged -= OnMapChanged;
    }

    private void Start()
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
    }
    
    public LayerMask highlighterMask; // 인스펙터에서 Highlighter 레이어만 체크
    private CastHighlighter _hovered;

    void Update()
    {
        // 캐스트 선택 중일 때만 동작
        if (currentSpell == null || !TurnManager.Instance.IsPlayerTurn()) return;

        // 마우스 → 월드
        var mpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 p = mpos;

        // 오직 Highlighter 레이어만 레이캐스트
        var hit = Physics2D.Raycast(p, Vector2.zero, 0f, highlighterMask);
        var ch = hit.collider ? hit.collider.GetComponent<CastHighlighter>() : null;

        // 호버 변경 처리(깜빡임 방지: 셀이 바뀔 때만 업데이트)
        if (ch != _hovered)
        {
            if (_hovered)
            {
                _hovered.SetHover(false);
                ClearSpellHighlights();
            }
            _hovered = ch;
            if (_hovered)
            {
                _hovered.SetHover(true);
                ShowSpellHighlights(_hovered.castCell);
            }
        }

        // 클릭 처리
        if (_hovered && Input.GetMouseButtonDown(0))
        {
            ConfirmCast(_hovered.castCell);
        }
    }

    private void ClearMoveHighlighters()
    {
        foreach (var highlighter in moveHighlighters) highlighter.SetActive(false);
    }

    public void HandleMoveHighlighterClick(Vector3 position)
    {
        AudioManager.Instance.PlaySFX(0);
        Vector3Int startCell = gridManager.WorldToCell(player.transform.position);
        Vector3Int targetCell = gridManager.WorldToCell(position);
        PlayerMoveRecorder.Instance.RecordMove(startCell, targetCell);
        StartCoroutine(Co_HandlePlayerMove(position));
    }

    private IEnumerator Co_HandlePlayerMove(Vector3 worldPos)
    {
        var cell = gridManager.WorldToCell(worldPos);
        var t = gridManager.GetTileData(cell);
        if (t != null && t.occupant && t.occupant.CompareTag("Scroll"))
            t.occupant.GetComponent<Scroll>()?.TryPickup(player);

        var mover = player.GetComponent<GridMoveVisual>();
        if (mover == null) mover = player.AddComponent<GridMoveVisual>();

        yield return mover.FakeThenSnap(gridManager, cell, useClaim:false);

        ClearMoveHighlighters();
        TurnManager.Instance.EndPlayerTurn();
    }
    
    public void ShowMoveHighlighters()
    {
        ClearCastHighlighters();
        ClearMoveHighlighters();

        Vector3Int playerCell = gridManager.backgroundMap.WorldToCell(player.transform.position);
        var movePositions = gridManager.GetWalkableNeighbors(playerCell);
        int index = 0;
        foreach (var cell in movePositions)
        {
            if (index >= moveHighlighters.Count) break;

            Vector3 worldPos = gridManager.backgroundMap.GetCellCenterWorld(cell.cellPosition);
            moveHighlighters[index].transform.position = worldPos;
            moveHighlighters[index].SetActive(true);
            index++;
        }
    }

    public void ShowCastHighlighters(SpellInstance spell)
    {
        ClearMoveHighlighters();
        ClearCastHighlighters();
        currentSpell = spell;

        Vector3Int playerCell = gridManager.backgroundMap.WorldToCell(player.transform.position);
        var castPositions = SpellPatterns.GetCastPositions(spell.data, playerCell);

        foreach (var cell in castPositions)
        {
            // HighlightManager.ShowCastHighlighters 안에서
            var worldPos = gridManager.backgroundMap.GetCellCenterWorld(cell);
            worldPos.z = -0.1f; // ← 카메라 쪽으로
            var go = Instantiate(castHighlighterPrefab, worldPos, Quaternion.identity);
            go.layer = LayerMask.NameToLayer("Highlighter");
            CastHighlighter cast = go.GetComponent<CastHighlighter>();
            cast.manager = this;
            cast.castCell = cell;
            castHighlighters.Add(cast);
        }
    }

    public void ShowSpellHighlights(Vector3Int castCell)
    {
        var areaCells = SpellPatterns.GetAreaPositions(
            currentSpell.data,
            gridManager.backgroundMap.WorldToCell(player.transform.position),
            castCell
        );

        foreach (var cell in areaCells)
        {
            Vector3 worldPos = gridManager.backgroundMap.GetCellCenterWorld(cell);
            GameObject go = Instantiate(spellHighlighterPrefab, worldPos, Quaternion.identity);
            SpellHighlighter spell = go.GetComponent<SpellHighlighter>();
            spell.Show();
            spellHighlighters.Add(spell);
        }
    }

    public void ClearCastHighlighters()
    {
        // 안전 가드
        
        
        foreach (var c in castHighlighters) Destroy(c.gameObject);
        castHighlighters.Clear();
    }

    public void ClearSpellHighlights()
    {
        foreach (var s in spellHighlighters) Destroy(s.gameObject);
        spellHighlighters.Clear();
    }

    private void OnMapChanged(MapContext ctx)
    {
        ClearCastHighlighters();
        ClearSpellHighlights();
        ClearMoveHighlighters();
        ShowMoveHighlighters();
    }

    public void ConfirmCast(Vector3Int castCell)
    {
        if (currentSpell == null) return;

        var playerCell = gridManager.WorldToCell(player.transform.position);
        var areaCells  = SpellPatterns.GetAreaPositions(currentSpell.data, playerCell, castCell);

        // 바라보는 방향 세팅
        var dir = castCell - playerCell;
        var pc  = player.GetComponent<PlayerController>();
        if (dir.x > 0)      pc.isTowardsRight = false;
        else if (dir.x < 0) pc.isTowardsRight = true;

        if (currentSpell.data.elementType == SpellElementType.Fire)
        {
            AudioManager.Instance.PlaySFX(1);
        }
        else if (currentSpell.data.elementType == SpellElementType.Wet)
        {
            AudioManager.Instance.PlaySFX(2);
        }
        else if (currentSpell.data.elementType == SpellElementType.Ice)
        {
            AudioManager.Instance.PlaySFX(3);
        }
        
        // 1) 공격 애니만 트리거
        PlayerAnimator.Instance.PlayAttackAnimation();

        // 2) 이펙트/데미지는 즉시 처리(원하면 이벤트로 옮겨도 됨)
        PlayCellEffects(currentSpell.data, areaCells);
        currentSpell.Cast(areaCells);

        // 3) 하이라이트 정리 + 기록
        ClearSpellHighlights();
        ClearCastHighlighters();
        PlayerMoveRecorder.Instance.RecordMove(PlayerMoveType.Attack, currentSpell.data.damage);

        // 4) ❌ 여기서 턴을 넘기지 않습니다
        //    (턴 종료는 애니메이션 이벤트에서만!)
    }
    
    private void PlayCellEffects(SpellData data, List<Vector3Int> areaCells)
    {
        if (data.effectPrefab == null) return;
        Transform parent = MapGenerator.Instance ? MapGenerator.Instance.mapInstance.transform : null;

        foreach (var cell in areaCells)
        {
            Vector3 pos = gridManager.CellToWorld(cell);
            Instantiate(data.effectPrefab, pos, Quaternion.identity, parent);
        }
    }
}
