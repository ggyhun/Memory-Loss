using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    
    [Header("References")]
    public GridManager gridManager;
    public HighlightManager highlightManager;
    
    [Header("Spells")]
    public List<SpellInstance> spells = new List<SpellInstance>();
    public SpellData normalAttackSpell;

    [Header("Animations")]
    public Animator animator;
    public GridMoveVisual mover;
    public SpriteRenderer rend;
    public bool isTowardsRight = false; // 플레이어가 바라보는 방향 (오른쪽이 기본)
    
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
    
    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // 초기화
        spells.Clear();
        isSpellSelected = false;
    }

    private void Start()
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (highlightManager == null) highlightManager = FindFirstObjectByType<HighlightManager>();
        if (animator == null) animator = GetComponent<Animator>();
        if (mover == null) mover = GetComponent<GridMoveVisual>();
        
        LearnSpell(normalAttackSpell);
        
        // 자식인 sprite의 Animator를 가져옴
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }
    
    public void OnMapChanged()
    {
        Debug.Log("OnMapChanged: PlayerController.cs");
        
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (highlightManager == null) highlightManager = FindFirstObjectByType<HighlightManager>();

        // 1) 하이라이트 매니저 먼저 초기화
        highlightManager?.Init(gameObject, gridManager);

        // 2) 플레이어 시작 위치 세팅 (점유자 포함)
        SetPlayerStartPosition();

        // 3) 플레이어 턴 시작(여기서 Move 하이라이트를 띄우게)
        TurnManager.Instance?.StartPlayerTurn();
    }
    
    private void SetPlayerStartPosition()
    {
        var startCell = gridManager.FindStartTilePosition();
        // 안전 가드
        if (startCell == Vector3Int.zero && (!gridManager.overlayMap || !gridManager.overlayMap.HasTile(startCell)))
        {
            Debug.LogWarning("SetPlayerStartPosition: startCell invalid. Skipping.");
            return;
        }

        // 이전 점유 해제
        var oldCell = gridManager.WorldToCell(transform.position);
        var oldTile = gridManager.GetTileData(oldCell);
        if (oldTile != null && oldTile.occupant == gameObject)
            gridManager.ClearOccupant(oldCell);

        // 🔴 오버레이 기준으로 월드 변환(없으면 백그라운드 사용)
        Vector3 worldPos =
            gridManager.overlayMap
                ? gridManager.overlayMap.GetCellCenterWorld(startCell)
                : gridManager.backgroundMap.GetCellCenterWorld(startCell);

        transform.position = worldPos;

        // 새 점유 설정(플레이어가 밟고 있는 칸은 보통 isWalkable=false)
        gridManager.SetOccupant(startCell, gameObject, false);

        Debug.Log($"Player start positioned at cell {startCell}, world {worldPos}");
        isSpellSelected = false;

        // ❌ 여기서 ShowMoveHighlighters() 호출하지 않음
        //    -> TurnManager.StartPlayerTurn()에서 호출되도록 유지
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

        if (mover.IsMoving)
        {
            animator.SetTrigger("Move");
        }
        rend.flipX = !isTowardsRight;
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
            if (!spell.CanCast())
            {
                Debug.Log($"{spell.data.spellName} is on cooldown ({spell.currentCooldown} turns left).");
                return;
            }
            
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
        
        // 플레이어가 턴을 시작할 때 모든 스킬의 쿨타운을 감소시킴
        ReduceCooldowns();

        if (!GetComponent<Stats>().CanAct)
        {
            PlayerMoveRecorder.Instance.RecordMove(PlayerMoveType.None);
            TurnManager.Instance.EndPlayerTurn();
            return;
        }
        
        highlightManager.ShowMoveHighlighters();
    }

    public void DeleteSpell(SpellInstance spell)
    {
        if (spells.Contains(spell))
        {
            spells.Remove(spell);
            Debug.Log($"Spell {spell.data.spellName} removed.");
        }
        else
        {
            Debug.LogWarning($"Spell {spell.data.spellName} not found in player's spell list.");
        }
    }
}
