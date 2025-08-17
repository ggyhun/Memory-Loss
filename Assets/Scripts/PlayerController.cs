using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    private Tilemap backgroundTilemap;
    public GridManager gridManager;
    public HighlightManager highlightManager;

    [Header("Spells")]
    public List<SpellInstance> spells = new List<SpellInstance>();

    public SpellData normalAttackSpell;

    private bool isSpellSelected;
    
    private Dictionary<KeyCode, int> spellKeyMap = new Dictionary<KeyCode, int>
    {
        { KeyCode.Alpha1, 0 },
        { KeyCode.Alpha2, 1 },
        { KeyCode.Alpha3, 2 },
        { KeyCode.Alpha4, 3 },
        { KeyCode.Alpha5, 4 },
        { KeyCode.Alpha6, 5 },
        { KeyCode.Alpha7, 6 },
        { KeyCode.Alpha8, 7 },
        { KeyCode.Alpha9, 8 },
    };

    private void Start()
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (backgroundTilemap == null)
        {
            backgroundTilemap = gridManager.backgroundMap;
            if (backgroundTilemap == null)
            {
                Debug.LogError("Background Tilemap not found in GridManager.");
            }
        }
        if (highlightManager == null) highlightManager = FindFirstObjectByType<HighlightManager>();

        Vector3Int startTilePosition = gridManager.FindStartTilePosition();
        transform.position = backgroundTilemap.GetCellCenterWorld(startTilePosition);

        isSpellSelected = false;
        
        highlightManager.ShowMoveHighlighters();
        LearnSpell(normalAttackSpell);
    }
    
    // PlayerController.cs (추가된 부분만)
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

    private void OnMapChanged(MapContext ctx)
    {
        // 참조 갱신
        backgroundTilemap = ctx.background;

        // GridManager는 이미 MapGenerator가 주입+리빌드를 끝냄
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();

        // 시작 위치 이동 (overlay/startTile이 있을 때)
        if (gridManager.overlayMap && gridManager.startTile)
        {
            var startCell = gridManager.FindStartTilePosition();
            transform.position = gridManager.CellToWorld(startCell);
        }

        // 하이라이트 초기화 등
        if (highlightManager == null) highlightManager = FindFirstObjectByType<HighlightManager>();
        highlightManager?.Init(gameObject, gridManager);

        // 필요시 플레이어 턴 시작
        TurnManager.Instance?.StartPlayerTurn();
    }

    private void Update()
    {
        if (!TurnManager.Instance.IsPlayerTurn()) return;

        foreach (var kvp in spellKeyMap)
        {
            if (Input.GetKeyDown(kvp.Key))
            {
                HandleSpellInput(kvp.Value);
            }
        }
    }
    
    private void HandleSpellInput(int index)
    {
        if (index >= spells.Count)
        {
            Debug.Log("No spell in that slot.");
            return;
        }

        var spell = spells[index];

        if (isSpellSelected)
        {
            Debug.Log("Spell selection cancelled.");
            isSpellSelected = false;
            highlightManager.ShowMoveHighlighters();
        }
        else
        {
            isSpellSelected = true;
            Debug.Log($"{spell.data.spellName} selected.");
            highlightManager.ShowCastHighlighters(spell);
        }
    }

    public void LearnSpell(SpellData newSpell)
    {
        spells.Add(new SpellInstance(newSpell));
        Debug.Log($"Learned new spell: {newSpell.spellName}");
    }

    public void ReduceCooldowns()
    {
        foreach (var spell in spells)
        {
            spell.TickCooldown();
        }
    }

    public void StartTurn()
    {
        Debug.Log("Player's turn started.");
        isSpellSelected = false;
        highlightManager.ShowMoveHighlighters();
        
        // 플레이어가 턴을 시작할 때 모든 스킬의 쿨타운을 감소시킴
        ReduceCooldowns();
    }
}
