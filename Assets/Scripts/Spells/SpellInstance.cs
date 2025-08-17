using UnityEngine;

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

    public void Cast()
    {
        if (!CanCast())
        {
            Debug.Log($"{data.spellName} on cooldown ({currentCooldown} turns left)");
            return;
        }

        // 실제 시전 로직 ...
        Debug.Log($"Casting {data.spellName}");

        // 스킬별 쿨타임 시작
        currentCooldown = Mathf.Max(0, data.cooldown);
    }

    public void TickCooldown(int amount = 1)
    {
        if (currentCooldown > 0)
            currentCooldown = Mathf.Max(0, currentCooldown - amount);
    }
}
