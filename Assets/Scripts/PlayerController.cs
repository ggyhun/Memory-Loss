using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Tilemap backgroundTilemap;
    public GridManager gridManager;
    public HighlightManager highlightManager;

    [Header("Spells")]
    public List<SpellInstance> spells = new List<SpellInstance>();

    public SpellData normalAttackSpell;

    private bool isSpellSelected;

    private void Awake()
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (highlightManager == null) highlightManager = FindFirstObjectByType<HighlightManager>();

        Vector3Int startTilePosition = gridManager.FindStartTilePosition();
        transform.position = backgroundTilemap.GetCellCenterWorld(startTilePosition);

        isSpellSelected = false;
        
        LearnSpell(normalAttackSpell);
    }

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
        }
        else
        {
            isSpellSelected = true;
            Debug.Log($"{spell.data.spellName} selected.");
            spell.Cast();
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
}
