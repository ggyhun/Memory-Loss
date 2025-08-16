public class SpellInstance
{
    public SpellData data;
    private int cooldown;
    private int currentCooldown;

    public SpellInstance(SpellData data)
    {
        this.data = data;
        cooldown = 3; // 예시값
        currentCooldown = 0;
    }

    public bool IsReady() => currentCooldown <= 0;

    public void TickCooldown()
    {
        if (currentCooldown > 0)
            currentCooldown--;
    }

    public void PutOnCooldown()
    {
        currentCooldown = cooldown;
    }
}
