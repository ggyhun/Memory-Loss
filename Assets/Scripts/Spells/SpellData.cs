using UnityEngine;
using System.Collections.Generic;

public enum CastPattern { Around4, Single, Diagonal, Around4Plus }
public enum AreaPattern { Single, LineForward2, Around4, Front1, Front2, Front3, Circle3, Splash }
public enum SpellType { Fire, Ice, Wet, None }

[CreateAssetMenu(fileName = "NewSpell", menuName = "Spells/New Spell")]
public class SpellData : ScriptableObject
{
    [Header("Basics")]
    public string spellName;
    public SpellType type;
    public int damage = 0;
    public int attackCount = 1; // 공격 횟수 (예: 연속 공격)
    public int cooldown = 0;
    public Sprite icon;
    
    [Header("Forgettable")]
    public bool isForgettable = true;   // 잊을 수 있는지 여부
    public int forgettableCooldown = 3; // 스킬 사용 후 잊혀지는 쿨다운  

    [Header("Cast Pattern (선택 가능한 칸)")]
    public CastPattern castPattern = CastPattern.Around4;

    [Header("Area Pattern (실제 데미지 범위)")]
    public AreaPattern areaPattern = AreaPattern.Single;

    [Header("Utility (소모형 버프 설정)")]
    public bool isUtility = false;                    // 유틸 여부
    public int utilityTurns = 3;                      // 지속 턴 수 (기본 3)
    [Range(1, 500)] public int utilityPercent = 120;  // 120 = +20%
    
    [Header("VFX / Prefabs (선택)")]
    public GameObject projectilePrefab;
    public GameObject effectPrefab;
}
