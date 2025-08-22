using System;
using UnityEngine;
using System.Collections.Generic;

public class HighlightManager : MonoBehaviour
{
    public GridManager gridManager;
    public TurnManager TurnManager;

    [Header("Prefabs")]
    public GameObject castHighlighterPrefab;
    public GameObject spellHighlighterPrefab;
    public GameObject moveHighlighterPrefab;

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
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>().gameObject;
        }

        if (gridManager == null)
        {
            gridManager = FindFirstObjectByType<GridManager>();
        }
        
        if (TurnManager == null)
        {
            TurnManager = FindFirstObjectByType<TurnManager>();
        }
        
        for (int i = 0; i < 4; i++)
        {
            GameObject moveHighlighter = Instantiate(moveHighlighterPrefab);
            moveHighlighter.GetComponent<MoveHighlighter>().highlightManager = this;
            moveHighlighter.SetActive(false);
            moveHighlighters.Add(moveHighlighter);
        }
    }
    
    
    private void OnDisable()
    {
        MapGenerator.Instance.OnMapChanged -= OnMapChanged;
    }

    private void Start()
    {
        MapGenerator.Instance.OnMapChanged += OnMapChanged;
        
        if (gridManager == null)
        {
            gridManager = FindFirstObjectByType<GridManager>();
        }
    }

    private void ClearMoveHighlighters()
    {
        foreach (var highlighter in moveHighlighters)
        {
            highlighter.SetActive(false);
        }
    }

    public void HandleMoveHighlighterClick(Vector3 position)
    {
        var t = gridManager.GetTileData(position);
        if (t.occupant && t.occupant.CompareTag("Scroll"))
        {
            var scroll = t.occupant.GetComponent<Scroll>();
            scroll.TryPickup(player);
        }

        PlayerMoveRecorder.Instance.RecordMove(PlayerMoveRecorder.Instance.CalculateMove(
            gridManager.WorldToCell(player.transform.position),
            gridManager.WorldToCell(position)
        ));
        
        gridManager.MoveTo(player, position);
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

    // 특정 SpellInstance 선택 시 CastHighlighter 표시
    public void ShowCastHighlighters(SpellInstance spell)
    {
        ClearMoveHighlighters();
        ClearCastHighlighters();
        currentSpell = spell;

        Vector3Int playerCell = gridManager.backgroundMap.WorldToCell(player.transform.position);
        var castPositions = SpellPatterns.GetCastPositions(spell.data,
            gridManager.backgroundMap.WorldToCell(player.transform.position));


        foreach (var cell in castPositions)
        {
            // if (!gridManager.GetTileData(cell)?.isWalkable ?? true) continue;

            Vector3 worldPos = gridManager.backgroundMap.GetCellCenterWorld(cell);
            GameObject go = Instantiate(castHighlighterPrefab, worldPos, Quaternion.identity);
            CastHighlighter cast = go.GetComponent<CastHighlighter>();
            cast.manager = this;
            cast.castCell = cell;
            castHighlighters.Add(cast);
        }
    }

    public void ShowSpellHighlights(Vector3Int castCell)
    {
        var areaCells = SpellPatterns.GetAreaPositions(currentSpell.data,
            gridManager.backgroundMap.WorldToCell(player.transform.position),
            castCell);

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
        foreach (var c in castHighlighters)
        {
            Destroy(c.gameObject);
        }
        castHighlighters.Clear();
    }

    public void ClearSpellHighlights()
    {
        foreach (var s in spellHighlighters)
        {
            Destroy(s.gameObject);
        }
        spellHighlighters.Clear();
    }

    private void OnMapChanged(MapContext ctx)
    {
        // 맵이 변경되면 하이라이트 모두 제거
        ClearCastHighlighters();
        ClearSpellHighlights();
        ClearMoveHighlighters();
        
        // 플레이어 위치에 맞춰 이동 하이라이트 다시 표시
        ShowMoveHighlighters();
    }

    public void ConfirmCast(Vector3Int castCell)
    {
        if (currentSpell == null) return;

        var playerCell = gridManager.WorldToCell(player.transform.position);

        // 1) 실제 타격 셀 계산
        var areaCells = SpellPatterns.GetAreaPositions(currentSpell.data, playerCell, castCell);

        // 2) 이펙트 재생 (각 셀에)
        PlayCellEffects(currentSpell.data, areaCells);

        // 3) 데미지/상태 적용
        currentSpell.Cast(areaCells);

        // 4) 정리 + 턴 종료
        ClearSpellHighlights();
        ClearCastHighlighters();
        PlayerMoveRecorder.Instance.RecordMove(PlayerMoveType.Attack, currentSpell.data.damage);
        TurnManager.Instance.EndPlayerTurn();
    }
    
    private void PlayCellEffects(SpellData data, List<Vector3Int> areaCells)
    {
        if (data.effectPrefab == null) return;

        Transform parent = MapGenerator.Instance ? MapGenerator.Instance.mapInstance.transform : null;

        foreach (var cell in areaCells)
        {
            Vector3 pos = gridManager.CellToWorld(cell);      // ✅ 셀 중심
            Instantiate(data.effectPrefab, pos, Quaternion.identity, parent);
        }
    }
}
