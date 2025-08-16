using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Spell Data")]
public class SpellData : ScriptableObject
{
    public string spellName;
    public int damage;
    public int cooldown;
    public int castHighlightType; // 1: 플레이어 주위 8칸 클릭 가능, 2: 플레이어 상하좌우 4칸 클릭 가능
    public GameObject[] spellHighlightPrefabs;
}