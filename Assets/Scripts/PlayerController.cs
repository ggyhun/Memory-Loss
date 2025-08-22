using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
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

    private void Start()
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (highlightManager == null) highlightManager = FindFirstObjectByType<HighlightManager>();
        if (animator == null) animator = GetComponent<Animator>();
        if (mover == null) mover = GetComponent<GridMoveVisual>();

        SetPlayerStartPosition();
        
        LearnSpell(normalAttackSpell);
        
        // 자식인 sprite의 Animator를 가져옴
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
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

    private void SetPlayerStartPosition()
    {
        Vector3Int startTilePosition = gridManager.FindStartTilePosition();
        transform.position = gridManager.backgroundMap.GetCellCenterWorld(startTilePosition);
        
        isSpellSelected = false;
        
        highlightManager.ShowMoveHighlighters();
    }

    private void OnMapChanged(MapContext _)
    {
        // GridManager는 이미 MapGenerator가 주입+리빌드를 끝냄
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();

        // 시작 위치 이동 (overlay/startTile이 있을 때)
        SetPlayerStartPosition();

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
