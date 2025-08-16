using UnityEngine;

[System.Serializable]
public class SpellInstance
{
    public SpellData data;
    public int currentCooldown;

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
            Debug.Log($"{data.spellName} is on cooldown ({currentCooldown} turns left).");
            return;
        }

        Debug.Log($"Casting {data.spellName}, Damage: {data.damage}");

        currentCooldown = data.cooldown;
    }

    public void TickCooldown()
    {
        if (currentCooldown > 0)
            currentCooldown--;
    }
}
