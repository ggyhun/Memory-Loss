using System.Collections.Generic;
using UnityEngine;

public class SpellInventory : MonoBehaviour
{
    // 플레이어가 보유한 스펠 목록
    [SerializeField] private List<SpellData> spells = new List<SpellData>();

    // 스펠 추가
    public void AddSpell(SpellData newSpell)
    {
        if (!spells.Contains(newSpell))
        {
            spells.Add(newSpell);
            Debug.Log($"{newSpell.spellName} 습득!");
        }
        else
        {
            Debug.Log($"{newSpell.spellName} 이미 보유 중");
        }
    }

    // 인덱스로 스펠 사용
    public void UseSpell(int index, Vector2Int playerPos)
    {
        if (index < 0 || index >= spells.Count)
        {
            Debug.LogWarning("잘못된 스펠 인덱스");
            return;
        }

        spells[index].Cast(playerPos, -1); // Todo: 하이라이트 인덱스는 나중에 구현 필요
    }

    // 보유 중인 모든 스펠 반환 (UI 등에서 사용)
    public List<SpellData> GetAllSpells()
    {
        return spells;
    }
}
