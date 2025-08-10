using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NormalAttack", menuName = "Spell/NormalAttack")]
public class NormalAttack : SpellData
{
    // 스펠 이름과 설명은 SpellData에서 상속받음
    
    // 공격 범위 [highlightIndex][directionIndex]
    public Vector2Int[][] ranges = new Vector2Int[][]
    {
        new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }, // 0: 상하좌우
        new Vector2Int[] { Vector2Int.up * 2, Vector2Int.down * 2, Vector2Int.left * 2, Vector2Int.right * 2 } // 1: 상하좌우 2칸
    };

    public override void Cast(Vector2Int playerPos, int highlightIndex)
    {
        // Todo: 하이라이트 인덱스에 따라 공격 범위 설정
    }
}
