using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SpellInstance
{
    private GameObject player;
    private Stats playerStats;
    private PlayerController playerController;
    public int inventroyIndex = -1; // 인벤토리 내 위치(런타임 상태)
    
    [Header("Cooldown & State")]
    public SpellData data;
    public int forgettableTurn; // 잊혀지는 쿨타임(런타임 상태)
    public bool isSealed = false; // 봉인 상태(런타임 상태)

    public SpellInstance(SpellData data)
    {
        this.data = data;
        forgettableTurn = data.forgettableCooldown;
        isSealed = false;
    }
    
    private void InitializePlayerReferences()
    {
        if (player == null)
        {
            player = PlayerController.Instance.gameObject;
            playerController = player.GetComponent<PlayerController>();
            playerStats = player.GetComponent<Stats>();
        }
    }

    public bool CanCast() => !isSealed;

    public void Cast(List<Vector3Int> spellCells)
    {  
        if (!CanCast())
        {
            Debug.Log("스킬을 시전할 수 없습니다.: 봉인 상태입니다.");
            return;
        }
        
        InitializePlayerReferences();

        if (!data.isUtility)
        {
            HandleAttackSpell(spellCells);
        }
        else
        {
            HandleUtilitySpell();
        }
        
        // 
    }

    private void HandleAttackSpell(List<Vector3Int> spellCells)
    {
        int damage = 0;
        ElementEffectType elementEffectType;
        switch (data.elementType)
        {
            case SpellElementType.Fire:
                damage = data.damage * playerStats.burningEnhancementAmount / 100; // ✅ 괄호 제거
                elementEffectType = ElementEffectType.Fire;
                break;
            case SpellElementType.Ice:
                damage = data.damage * playerStats.frozenEnhancementAmount / 100;  // ✅ 괄호 제거
                elementEffectType = ElementEffectType.Ice;
                break;
            case SpellElementType.Wet:
                damage = data.damage * playerStats.wetEnhancementAmount / 100;     // ✅ 괄호 제거
                elementEffectType = ElementEffectType.Water;
                break;
            default:
                damage = data.damage;
                elementEffectType = ElementEffectType.Normal;
                break;
        }

        if (spellCells != null)
        {
            foreach (var cell in spellCells)
            {
                var target = GridManager.Instance.GetOccupant(cell);
                if (target == null) continue;
                var targetStats = target.GetComponent<Stats>();
                if (targetStats == null) continue;

                targetStats.TakeDamage(damage, elementEffectType);
            }
        }
    }
    
    private void HandleUtilitySpell()
    {
        switch (data.spellType)
        {
            case SpellType.Enhance:
            {
                if (playerStats != null)
                {
                    switch (data.elementType)
                    {
                        case SpellElementType.Fire:
                            playerStats.ApplyEnhance(ElementEffectType.Fire,  data.utilityPercent, data.utilityTurns);
                            break;
                        case SpellElementType.Ice:
                            playerStats.ApplyEnhance(ElementEffectType.Ice,   data.utilityPercent, data.utilityTurns);
                            break;
                        case SpellElementType.Wet:
                            playerStats.ApplyEnhance(ElementEffectType.Water, data.utilityPercent, data.utilityTurns);
                            break;
                    }
                }
                break;
            }
            case SpellType.Recollection:
            {
                playerController.AddCooldownsToAll();
                break;
            }
            case SpellType.Heal:
            {
                playerStats.Heal(playerStats.maxHp * 10 / 100);
                break;
            }
            case SpellType.ExtraTurn:
            {
                TurnManager.Instance.SetExtraTurn();
                break;
            }
            case SpellType.Distortion:
            {
                // TODO:
                break;
            }
            case SpellType.UpMaxHp:
            {
                playerStats.IncreaseMaxHp();
                break;
            }
        }
        
        DeleteSpell();
    }

    public void Seal()
    {
        isSealed = true;
    }

    public void TickCooldown(int amount = 1)
    {
        forgettableTurn = Mathf.Max(0, forgettableTurn - amount);
        
        
    }
    
    public void AddCooldown(int amount)
    {
        forgettableTurn = Mathf.Max(0, forgettableTurn + amount); // 최소 1턴 증가, 자신 턴 보정 1
    }

    public void DeleteSpell()
    {
        PlayerController.Instance.DeleteSpell(inventroyIndex);
    }
}
