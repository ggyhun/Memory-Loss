using UnityEngine;
using System.Collections.Generic;

public enum CastPattern { Around8, Cross4, Line, Ring, Custom }
public enum AreaPattern { Single, LineForwardN, CrossPlus, SquareNxN, Custom }

[CreateAssetMenu(fileName = "NewSpell", menuName = "Spells/New Spell")]
public class SpellData : ScriptableObject
{
    [Header("Basics")]
    public string spellName;
    public int cooldown = 0;

    [Header("Cast Pattern (선택 가능한 칸)")]
    public CastPattern castPattern = CastPattern.Around8;
    public int castRange = 1;                // Line, Ring 등에 사용
    public List<Vector2Int> castCustomOffsets; // Custom일 때만 사용 (플레이어 기준 상대좌표)

    [Header("Area Pattern (실제 데미지 범위)")]
    public AreaPattern areaPattern = AreaPattern.Single;
    public int areaSize = 1;                 // N(라인 길이 / 정사각형 크기 등)
    public List<Vector2Int> areaCustomOffsets; // Custom일 때 사용 (선택칸 또는 방향 기준 상대좌표)

    [Header("VFX / Prefabs (선택)")]
    public GameObject projectilePrefab;
    public GameObject effectPrefab;
}
