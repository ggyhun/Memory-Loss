using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

[System.Serializable]
public class SpellInstance
{
    private GameObject player;
    private Stats playerStats;
    private PlayerController playerController;
    
    public SpellData data;
    public int currentCooldown;  // 남은 쿨타임(런타임 상태)
    

    public SpellInstance(SpellData data)
    {
        this.data = data;
        currentCooldown = 0;
    }

    public bool CanCast() => currentCooldown <= 0;

    public void Cast(List<Vector3Int> spellCells)
    {  
        if (!CanCast())
        {
            Debug.LogError($"{data.spellName} on cooldown ({currentCooldown} turns left)");
            return;
        }

        Init(GameObject.FindWithTag("Player"));
        var spellType = data.type;

        // ---- 유틸(소모형) ----
        if (data.isUtility)
        {
            if (playerStats != null)
            {
                switch (spellType)
                {
                    case SpellType.Fire:
                        playerStats.ApplyEnhance(ElementType.Fire,  data.utilityPercent, data.utilityTurns);
                        break;
                    case SpellType.Ice:
                        playerStats.ApplyEnhance(ElementType.Ice,   data.utilityPercent, data.utilityTurns);
                        break;
                    case SpellType.Wet:
                        playerStats.ApplyEnhance(ElementType.Water, data.utilityPercent, data.utilityTurns);
                        break;
                }
            }
            // 인벤토리에 유지: 쿨다운만 시작 (0이 되면 삭제는 TickCooldown에서)
            currentCooldown = Mathf.Max(0, data.cooldown);
            return;
        }

        // ---- 공격 스펠 ----
        int damage = 0;
        ElementType elementType = ElementType.Normal;
        switch (spellType)
        {
            case SpellType.Fire:
                damage = data.damage * playerStats.burningEnhancementAmount / 100; // ✅ 괄호 제거
                elementType = ElementType.Fire;
                break;
            case SpellType.Ice:
                damage = data.damage * playerStats.frozenEnhancementAmount / 100;  // ✅ 괄호 제거
                elementType = ElementType.Ice;
                break;
            case SpellType.Wet:
                damage = data.damage * playerStats.wetEnhancementAmount / 100;     // ✅ 괄호 제거
                elementType = ElementType.Water;
                break;
            default:
                damage = data.damage;
                elementType = ElementType.Normal;
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

                targetStats.TakeDamage(damage, elementType);
            }
        }

        currentCooldown = Mathf.Max(0, data.cooldown);
    }


    public void TickCooldown(int amount = 1)
    {
        if (currentCooldown > 0)
        {
            currentCooldown = Mathf.Max(0, currentCooldown - amount);

            // 유틸 스펠: 쿨다운이 0이 되면 인벤토리에서 삭제
            if (data.isUtility && currentCooldown == 0)
            {
                Init(GameObject.FindWithTag("Player"));
                playerController?.DeleteSpell(this);
            }
        }
    }
    
    public void Init(GameObject player)
    {
        this.player = player;
        playerStats = player.GetComponent<Stats>();
        playerController = player.GetComponent<PlayerController>();
    }
}
