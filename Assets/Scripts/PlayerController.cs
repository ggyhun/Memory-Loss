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
    public bool isTowardsRight = false; // 플레이어가 바라보는 방향

    private bool isSpellSelected;

    // --- 인벤토리 UI 캐시 ---
    private bool _invUiCached = false;
    private Image[] _slotIcons = new Image[9];
    private TextMeshProUGUI[] _slotCounts = new TextMeshProUGUI[9];

    private Dictionary<KeyCode, int> spellKeyMap = new Dictionary<KeyCode, int>
    {
        { KeyCode.Alpha1, 0 }, { KeyCode.Alpha2, 1 }, { KeyCode.Alpha3, 2 },
        { KeyCode.Alpha4, 3 }, { KeyCode.Alpha5, 4 }, { KeyCode.Alpha6, 5 },
        { KeyCode.Alpha7, 6 }, { KeyCode.Alpha8, 7 }, { KeyCode.Alpha9, 8 },
    };

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        spells.Clear();
        isSpellSelected = false;
    }

    private void Start()
    {
        if (gridManager == null)       gridManager = FindFirstObjectByType<GridManager>();
        if (highlightManager == null)  highlightManager = FindFirstObjectByType<HighlightManager>();
        if (animator == null)          animator = GetComponent<Animator>();
        if (mover == null)             mover = GetComponent<GridMoveVisual>();

        // 자식에 Animator가 있으면 사용
        if (animator == null) animator = GetComponentInChildren<Animator>();

        // 기본 공격 스펠 학습(슬롯 0을 의도)
        if (normalAttackSpell != null)
            LearnSpell(normalAttackSpell);
    }

    private bool isMoving = false;
    private void Update()
    {
        if (!TurnManager.Instance.IsPlayerTurn()) return;

        foreach (var kvp in spellKeyMap)
            if (Input.GetKeyDown(kvp.Key))
                HandleSpellInput(kvp.Value);

        if (mover && mover.IsMoving)
        {
            if (!isMoving)
            {
                isMoving = true;
                PlayerAnimator.Instance.PlayerMoveAnimation();
            }
        }
        else if (isMoving)
        {
            isMoving = false;
            TurnManager.Instance.NotifyPlayerAnimationComplete();
        }

        if (rend) rend.flipX = !isTowardsRight;

        UpdateInventoryUI();
    }

    private void UpdateInventoryUI()
    {
        // 1) UI 캐시
        if (!_invUiCached)
        {
            GameObject slotsGO = GameObject.Find("Slots");
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
                    var iconTr  = slot.Find("Icon");
                    var countTr = slot.Find("Count");
                    if (iconTr)  _slotIcons[i]  = iconTr.GetComponent<Image>();
                    if (countTr) _slotCounts[i] = countTr.GetComponent<TextMeshProUGUI>();
                }
                _invUiCached = true;
            }
            else
            {
                return; // 이번 프레임은 스킵하고 다음 프레임에 재시도
            }
        }

        // 2) 내용 반영
        for (int i = 0; i < 9; i++)
        {
            Image iconImg = _slotIcons[i];
            TextMeshProUGUI countTxt = _slotCounts[i];

            if (i < spells.Count)
            {
                var inst = spells[i];
                if (iconImg)
                {
                    iconImg.sprite = (inst != null && inst.data != null) ? inst.data.icon : null;
                    iconImg.enabled = (iconImg.sprite != null);
                    iconImg.preserveAspect = true;
                }
                if (countTxt)
                {
                    int n = (inst != null) ? inst.forgettableTurn : 0;
                    countTxt.text = (n >= 1) ? n.ToString() : "0";
                }
            }
            else
            {
                if (iconImg)  { iconImg.sprite = null; iconImg.enabled = false; }
                if (countTxt) countTxt.text = "";
            }
        }
    }

    public void OnMapChanged()
    {
        Debug.Log("OnMapChanged: PlayerController.cs");

        if (gridManager == null)      gridManager = FindFirstObjectByType<GridManager>();
        if (highlightManager == null) highlightManager = FindFirstObjectByType<HighlightManager>();

        highlightManager?.Init(gameObject, gridManager);
        SetPlayerStartPosition();
        TurnManager.Instance?.StartPlayerTurn();
    }

    private void SetPlayerStartPosition()
    {
        var startCell = gridManager.FindStartTilePosition();

        if (startCell == Vector3Int.zero &&
            (!gridManager.overlayMap || !gridManager.overlayMap.HasTile(startCell)))
        {
            Debug.LogWarning("SetPlayerStartPosition: startCell invalid. Skipping.");
            return;
        }

        var oldCell = gridManager.WorldToCell(transform.position);
        var oldTile = gridManager.GetTileData(oldCell);
        if (oldTile != null && oldTile.occupant == gameObject)
            gridManager.ClearOccupant(oldCell);

        Vector3 worldPos = gridManager.overlayMap
            ? gridManager.overlayMap.GetCellCenterWorld(startCell)
            : gridManager.backgroundMap.GetCellCenterWorld(startCell);

        transform.position = worldPos;
        gridManager.SetOccupant(startCell, gameObject, false);

        Debug.Log($"Player start positioned at cell {startCell}, world {worldPos}");
        isSpellSelected = false;
    }

    private void HandleSpellInput(int index)
    {
        if (index >= spells.Count) { Debug.Log("No spell in that slot."); return; }

        var spell = spells[index];

        if (spell.isSealed) { Debug.Log("Spell is sealed."); return; }

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

        var inst = new SpellInstance(newSpell);
        spells.Add(inst);
        UpdateInventoryIndex();

        Debug.Log($"Learned new spell: {newSpell.spellName}");
    }

    public void ReduceCooldowns()
    {
        Debug.Log("PC: Reducing cooldowns for all spells.");

        // 스냅샷 순회: 내부에서 삭제가 일어나도 안전
        var snapshot = spells.ToArray();
        foreach (var s in snapshot)
        {
            if (s != null) s.TickCooldown();
        }

        // (선택) 여기서 추가 정리 필요 시 역순 Remove 가능
        // for (int i = spells.Count - 1; i >= 0; --i) { ... }
    }

    public void StartTurn()
    {
        Debug.Log("Player's turn started.");
        isSpellSelected = false;

        // 턴 시작 시 잊혀지는 쿨타임 감소
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
        // 스냅샷 순회(여긴 삭제 일어나지 않지만 동일 패턴 유지)
        var snapshot = spells.ToArray();
        foreach (var s in snapshot)
            if (s != null) s.AddCooldown(amount);
    }

    // 외부/SpellInstance가 호출
    public void DeleteSpell(int index)
    {
        if (index < 0 || index >= spells.Count)
        {
            // 인덱스가 오래되었을 수 있으므로 인벤토리 인덱스로 찾아보기
            var byIndex = spells.Find(s => s != null && s.inventroyIndex == index);
            if (byIndex == null)
            {
                Debug.Log($"Invalid spell index: {index}");
                return;
            }
            spells.Remove(byIndex);
            if (byIndex.data && byIndex.data.isForgettable) spellHistory.Add(byIndex.data);
            UpdateInventoryIndex();
            return;
        }

        var spell = spells[index];
        if (spell != null && spell.data != null && spell.data.isForgettable)
            spellHistory.Add(spell.data);

        spells.RemoveAt(index);
        Debug.Log($"Forgotten spell (idx {index}): {(spell?.data?.spellName ?? "null")}");
        UpdateInventoryIndex();
    }

    // 필요시 참조로도 삭제 가능
    public void DeleteSpell(SpellInstance inst)
    {
        int idx = spells.IndexOf(inst);
        if (idx >= 0) DeleteSpell(idx);
    }

    public bool SealRandomSpell(int playerSpellsCount)
    {
        if (spells.Count < playerSpellsCount) return false;

        var available = spells.FindAll(s => !s.isSealed && s.data != normalAttackSpell);
        if (available.Count == 0) { Debug.Log("No available spells to seal."); return false; }

        int r = Random.Range(0, available.Count);
        available[r].Seal();
        Debug.Log($"Spell {available[r].data.spellName} has been sealed.");
        return true;
    }

    public void UpdateInventoryIndex()
    {
        for (int i = 0; i < spells.Count; i++)
            spells[i].inventroyIndex = i;
    }

    public void LearnRecentForgottenSpell()
    {
        if (spellHistory.Count == 0)
        {
            Debug.Log("No recently forgotten spells.");
            return;
        }

        var recent = spellHistory[spellHistory.Count - 1];
        spellHistory.RemoveAt(spellHistory.Count - 1);
        LearnSpell(recent);
        UpdateInventoryIndex();
    }
}
