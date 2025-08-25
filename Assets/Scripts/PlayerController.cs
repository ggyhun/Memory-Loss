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

    // --- 인벤토리 UI 캐시 & 툴팁 ---
    private bool _invUiCached = false;
    private Image[] _slotIcons = new Image[9];
    private TextMeshProUGUI[] _slotCounts = new TextMeshProUGUI[9];
    private RectTransform[] _slotRects = new RectTransform[9];
    private Canvas _uiCanvas;

    private RectTransform _tooltipRect;
    private TextMeshProUGUI _tooltipText;
    private int _hoveredIndex = -1;

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
        // =========================
        //  인벤토리 UI 업데이트
        // =========================
        if (!_invUiCached)
        {
            // 1) 슬롯 컨테이너 찾기: "Slots" (InventoryBase 하위라면 "InventoryBase/Slots")
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
                _uiCanvas = slotsGO.GetComponentInParent<Canvas>();
                var root = slotsGO.transform;
                int cnt = Mathf.Min(9, root.childCount);

                for (int i = 0; i < cnt; i++)
                {
                    var slot = root.GetChild(i);
                    _slotRects[i] = slot as RectTransform;

                    var iconTr = slot.Find("Icon");
                    if (iconTr) _slotIcons[i] = iconTr.GetComponent<Image>();

                    var countTr = slot.Find("Count");
                    if (countTr) _slotCounts[i] = countTr.GetComponent<TextMeshProUGUI>();
                }

                _invUiCached = true;
            }
            else
            {
                // 못 찾으면 다음 프레임에 재시도
                return;
            }
        }

        // 2) spells → 아이콘/카운트 반영 (icon = data.icon, count = forgettableTurn)
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
                    // 아이콘은 클릭 막지 않도록(슬롯이 이벤트 받게) 필요시 꺼두기
                    // iconImg.raycastTarget = false;
                }

                if (countTxt)
                {
                    int n = (inst != null) ? inst.forgettableTurn : 0;
                    countTxt.text = (n > 1) ? n.ToString() : "";
                }
            }
            else
            {
                if (iconImg) { iconImg.sprite = null; iconImg.enabled = false; }
                if (countTxt) countTxt.text = "";
            }
        }

        // 3) 마우스 호버한 슬롯 찾기 → 툴팁 표시/숨김
        int newHover = -1;
        for (int i = 0; i < 9; i++)
        {
            var rt = _slotRects[i];
            if (!rt) continue;

            Camera cam = (_uiCanvas && _uiCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? _uiCanvas.worldCamera : null;
            if (RectTransformUtility.RectangleContainsScreenPoint(rt, Input.mousePosition, cam))
            {
                newHover = i;
                break;
            }
        }

        if (newHover != _hoveredIndex)
        {
            _hoveredIndex = newHover;
            if (newHover >= 0) ShowTooltip(newHover);
            else HideTooltip();
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
    
    // 툴팁 오브젝트를 필요 시 자동 생성
    private void EnsureTooltip()
    {
        if (_tooltipRect) return;

        _uiCanvas = _uiCanvas ?? FindFirstObjectByType<Canvas>();

        var go = new GameObject("SpellTooltip", typeof(RectTransform), typeof(Image));
        _tooltipRect = go.GetComponent<RectTransform>();
        _tooltipRect.SetParent(_uiCanvas.transform, false);
        _tooltipRect.pivot = new Vector2(0.5f, 0f);                   // 아래쪽 가운데 기준
        _tooltipRect.anchorMin = _tooltipRect.anchorMax = new Vector2(0.5f, 0.5f);

        var bg = go.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.85f);                      // 반투명 검정
        bg.raycastTarget = false;

        var textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        var tr = textGO.GetComponent<RectTransform>();
        tr.SetParent(_tooltipRect, false);
        tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 0.5f);
        tr.pivot = new Vector2(0.5f, 0.5f);

        _tooltipText = textGO.GetComponent<TextMeshProUGUI>();
        _tooltipText.alignment = TextAlignmentOptions.Center;
        _tooltipText.enableWordWrapping = false;
        _tooltipText.fontSize = 18;
        _tooltipText.raycastTarget = false;

        _tooltipRect.gameObject.SetActive(false);
    }

    private void ShowTooltip(int index)
    {
        if (index < 0 || index >= spells.Count || spells[index] == null || spells[index].data == null)
        {
            HideTooltip();
            return;
        }

        EnsureTooltip();

        string name = spells[index].data.spellName ?? "";
        if (string.IsNullOrEmpty(name)) { HideTooltip(); return; }

        _tooltipText.text = name;

        // 텍스트 길이에 맞게 툴팁 박스 크기 조정 (패딩 포함)
        Vector2 pref = _tooltipText.GetPreferredValues(name);
        float padX = 16f, padY = 10f;
        _tooltipRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pref.x + padX);
        _tooltipRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   pref.y + padY);

        // 슬롯의 상단 중앙 좌표 바로 위(8px)에 배치
        var rt = _slotRects[index];
        if (!rt) { HideTooltip(); return; }

        Vector3[] corners = new Vector3[4]; // 0:BL,1:TL,2:TR,3:BR
        rt.GetWorldCorners(corners);
        Vector3 topCenter = (corners[1] + corners[2]) * 0.5f + new Vector3(0f, 8f, 0f);

        Camera cam = (_uiCanvas && _uiCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? _uiCanvas.worldCamera : null;
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, topCenter);
        _tooltipRect.position = screenPos;

        _tooltipRect.gameObject.SetActive(true);
    }

    private void HideTooltip()
    {
        if (_tooltipRect) _tooltipRect.gameObject.SetActive(false);
    }
}
