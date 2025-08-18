using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SpellInstance
{
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
        
        var spellType = data.type;
        
        GameObject player = GameObject.FindWithTag("Player");
        Stats playerStats = player.GetComponent<Stats>();
        
        GameObject target;
        Stats targetStats;
        
        // Damage Cal
        int damage = 0;
        ElementType elementType = ElementType.Normal;
        switch (spellType)
        {
            case SpellType.Fire:
            {
                damage = data.damage * (playerStats.burningEnhancementAmount / 100);
                elementType = ElementType.Fire;
                break;
            }
            case SpellType.Ice:
            {
                damage = data.damage * (playerStats.frozenEnhancementAmount / 100);
                elementType = ElementType.Ice;
                break;
            }
            case SpellType.Wet:
            {
                damage = data.damage * (playerStats.wetEnhancementAmount / 100);
                elementType = ElementType.Water;
                break;
            }
        }
        
        foreach (var cell in spellCells)
        {
            target = GridManager.Instance.GetOccupant(cell);
            targetStats = target.GetComponent<Stats>();
            if (targetStats == null)
            {
                continue;
            }
            targetStats.TakeDamage(damage, elementType);
        }
        // 스킬별 쿨타임 시작
        currentCooldown = Mathf.Max(0, data.cooldown);
    }

    public void TickCooldown(int amount = 1)
    {
        if (currentCooldown > 0)
            currentCooldown = Mathf.Max(0, currentCooldown - amount);
    }
}
