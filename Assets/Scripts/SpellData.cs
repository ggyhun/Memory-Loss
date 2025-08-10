using UnityEngine;

[CreateAssetMenu(fileName = "Spell Data", menuName = "Spell/SpellData")]
public abstract class SpellData : ScriptableObject
{
    public string spellName; // 스펠 이름
    public string description; // 스펠 설명
    public int cooldown;
    public Sprite icon; // 스펠 아이콘

    public abstract void Cast(Vector2Int playerPos, int highlightIndex);
}
