using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSpell", menuName = "Spells/SpellData")]
public class SpellData : ScriptableObject
{
    public string spellName;
    public Sprite icon;
    public GameObject projectilePrefab;
    public int maxRange = 2;

    // Cast 가능한 위치 패턴 계산
    public virtual List<Vector3Int> GetCastPositions(Vector3Int playerCell)
    {
        List<Vector3Int> result = new List<Vector3Int>();

        // 예시: 플레이어 기준 maxRange 이내 대각선
        for (int r = 1; r <= maxRange; r++)
        {
            result.Add(playerCell + new Vector3Int(r, r, 0));
            result.Add(playerCell + new Vector3Int(-r, r, 0));
            result.Add(playerCell + new Vector3Int(r, -r, 0));
            result.Add(playerCell + new Vector3Int(-r, -r, 0));
        }

        return result;
    }

    // 특정 Cast 위치 기준 실제 Spell 범위 계산
    public virtual List<Vector3Int> GetSpellArea(Vector3Int castCell)
    {
        List<Vector3Int> result = new List<Vector3Int>();
        result.Add(castCell); // 예시: Cast 지점 본인
        return result;
    }
}
