using UnityEngine;

[CreateAssetMenu(fileName = "Spell Data", menuName = "ScriptableObjects/SpellData", order = int.MaxValue)]
public abstract class SpellData : ScriptableObject
{
    [Header("기본 정보")]
    public string spellName;
    public Sprite icon;
    public int damage;
    public int cooldownTurns;

    /// <summary>
    /// 스펠 발동 메서드
    /// </summary>
    /// <param name="casterPos">시전자 위치</param>
    /// <param name="highlightIndex">하이라이트된 타일 또는 방향 인덱스</param>
    public abstract void Cast(Vector2Int casterPos, int highlightIndex);
}