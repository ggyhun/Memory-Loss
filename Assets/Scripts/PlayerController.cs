using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    
    [Header("References")]
    public GridManager gridManager;
    public HighlightManager highlightManager;
    
    [Header("Spells")]
    public List<SpellInstance> spells = new List<SpellInstance>();
    public SpellData normalAttackSpell;
    [SerializeField] private List<SpellData> spellHistory = new List<SpellData>();

    [Header("Animations")]
    public Animator animator;
    public GridMoveVisual mover;
    public SpriteRenderer rend;
    public bool isTowardsRight = false; // 플레이어가 바라보는 방향 (오른쪽이 기본)
    
    private bool isSpellSelected;

    
    // --- 인벤토리 UI 캐시용 ---
    private bool _invUiCached = false;
    private Image[] _slotIcons = new Image[9];
    private TextMeshProUGUI[] _slotCounts = new TextMeshProUGUI[9];
    
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
    
    private bool isMoving = false;
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
        isMoving = true;
        PlayerAnimator.Instance.PlayerMoveAnimation();
    }
    if (!mover.IsMoving && isMoving)
    {
        isMoving = false;
        TurnManager.Instance.NotifyPlayerAnimationComplete(); // 애니 끝 신호
    }

    rend.flipX = !isTowardsRight;

    // =========================
    //  인벤토리 UI 업데이트
    // =========================
    // 1) 한 번만 슬롯 참조 캐시(이름 규칙: "Slots" 컨테이너 안에 Slot(0..8), 각 Slot 안에 "Icon", "Count")
    if (!_invUiCached)
    {
        // 1-1) Slots 컨테이너 찾기 (권장 이름: "Slots")
        GameObject slotsGO = GameObject.Find("Slots");

        // 이름이 다르다면 InventoryBase 하위에서 그리드처럼 보이는 오브젝트를 대충 찾아봄(폴백)
        if (slotsGO == null)
        {
            var canvases = GameObject.FindObjectsOfType<Canvas>();
            foreach (var c in canvases)
            {
                var t = c.transform.Find("InventoryBase/Slots");
                if (t != null) { slotsGO = t.gameObject; break; }
                var t2 = c.transform.Find("Slots");
                if (t2 != null) { slotsGO = t2.gameObject; break; }
            }
        }

        if (slotsGO != null)
        {
            var root = slotsGO.transform;
            int count = Mathf.Min(9, root.childCount);
            for (int i = 0; i < count; i++)
            {
                var slot = root.GetChild(i);

                // 자식 "Icon" 이미지
                var iconTr = slot.Find("Icon");
                if (iconTr != null) _slotIcons[i] = iconTr.GetComponent<Image>();

                // 자식 "Count" TMP 텍스트(없으면 null 허용)
                var countTr = slot.Find("Count");
                if (countTr != null) _slotCounts[i] = countTr.GetComponent<TextMeshProUGUI>();
            }
            _invUiCached = true;
        }
        else
        {
            // 슬롯 못 찾으면 이번 프레임은 스킵 (다음 프레임에 다시 시도)
            return;
        }
    }

    // 2) spells -> UI 반영(아이콘 & 카운트)
    for (int i = 0; i < 9; i++)
    {
        Image iconImg = _slotIcons[i];
        TextMeshProUGUI countTxt = _slotCounts[i];

        if (i < spells.Count)
        {
            var inst = spells[i];
            // 아이콘
            if (iconImg)
            {
                iconImg.sprite = (inst != null && inst.data != null) ? inst.data.icon : null;
                iconImg.enabled = (iconImg.sprite != null);
                iconImg.preserveAspect = true;
                // 필요하면 iconImg.raycastTarget = false;  // 슬롯 클릭 충돌 방지
            }

            // 카운트 = forgettableTurn (1 이하면 빈칸)
            if (countTxt)
            {
                int n = (inst != null) ? inst.forgettableTurn : 0;
                countTxt.text = (n > 1) ? n.ToString() : "0";
            }
        }
        else
        {
            // 빈 칸 처리
            if (iconImg) { iconImg.sprite = null; iconImg.enabled = false; }
            if (countTxt) countTxt.text = "";
        }
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
    
    
    private void HandleSpellInput(int index)
    {
        if (index >= spells.Count)
        {
            Debug.Log("No spell in that slot.");
            return;
        }

        var spell = spells[index];

        // 봉인된 스킬은 선택 불가
        if (spell.isSealed)
        {
            Debug.Log("Spell is sealed.");
            return;
        }

        // 이미 스킬이 선택된 상태에서 같은 스킬을 다시 누르면 선택 취소
        if (isSpellSelected)
        {
            Debug.Log("Spell selection cancelled.");
            isSpellSelected = false;
            highlightManager.ShowMoveHighlighters();
        }
        else
        {
            if (!spell.CanCast()) return;
            isSpellSelected = true;
            Debug.Log($"{spell.data.spellName} selected.");
            highlightManager.ShowCastHighlighters(spell);
        }
    }

    public void LearnSpell(SpellData newSpell)
    {
        if (spells.Count >= 9)
        {
            Debug.Log("Cannot learn more spells: inventory full.");
            return;
        }
        
        spells.Add(new SpellInstance(newSpell));
        Debug.Log($"Learned new spell: {newSpell.spellName}");
        UpdateInventoryIndex();
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

    public void AddCooldownsToAll(int amount = 1)
    {
        foreach (var spell in spells)
        {
            spell.AddCooldown(amount);
        }
    }

    public void DeleteSpell(int index)
    {
        if (index < 0 || index >= spells.Count)
        {
            Debug.Log("Invalid spell index.");
            return;
        }

        var spell = spells[index];
        if (!spell.data.isForgettable)
        {
            Debug.Log("This spell cannot be forgotten.");
            return;
        }
        
        // 삭제시 History에 추가
        spellHistory.Add(spell.data);
        spells.RemoveAt(index);
        Debug.Log($"Forgotten spell: {spell.data.spellName}");
        UpdateInventoryIndex();
    }

    public bool SealRandomSpell(int playerSpellsCount)
    {
        if (spells.Count < playerSpellsCount) return false;
        
        var availableSpells = spells.FindAll(s => !s.isSealed && s.data != normalAttackSpell);
        if (availableSpells.Count == 0)
        {
            
            Debug.Log("No available spells to seal.");
            return false;
        }

        var randomIndex = Random.Range(0, availableSpells.Count);
        var spellToSeal = availableSpells[randomIndex];
        spellToSeal.Seal();

        Debug.Log($"Spell {spellToSeal.data.spellName} has been sealed.");
        
        return true;
    }

    public void UpdateInventoryIndex()
    {
        int index = 0;
        bool normalAttackSign = true;
        foreach (var spell in spells)
        {
            spell.inventroyIndex = index;
        }
    }
    
    public void LearnRecentForgottenSpell()
    {
        if (spellHistory.Count == 0)
        {
            Debug.Log("No recently forgotten spells.");
            return;
        }

        var recentSpell = spellHistory[spellHistory.Count - 1];
        LearnSpell(recentSpell);
        UpdateInventoryIndex();
    }
}
