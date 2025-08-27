using UnityEngine;

public enum CastPattern { Around4, Single, Diagonal, Around4Plus, Around8 }
public enum AreaPattern { Single, LineForward2, Around4, Front1, Front2, Front3, Circle3, Splash }
public enum SpellElementType { Fire, Ice, Wet, None }
public enum SpellType {Attack, Enhance, Recollection, Heal, ExtraTurn, Distortion, UpMaxHp}

[CreateAssetMenu(fileName = "NewSpell", menuName = "Spells/New Spell")]
public class SpellData : ScriptableObject
{
    [Header("Basics")]
    public string spellName;
    public SpellElementType elementType;
    public int damage = 0;
    public Sprite icon;
    
    [Header("Forgettable Cooldown")]
    public bool isForgettable = true;   // 잊을 수 있는지 여부
    public int forgettableCooldown = 3; // 스킬 사용 후 잊혀지는 쿨다운

    [Header("Cast Pattern (선택 가능한 칸)")]
    public CastPattern castPattern = CastPattern.Around4;

    [Header("Area Pattern (실제 데미지 범위)")]
    public AreaPattern areaPattern = AreaPattern.Single;

    [Header("Utility (소모형 버프 설정)")]
    public bool isUtility = false;
    public SpellType spellType = SpellType.Attack;      // 유틸 여부
    public int utilityTurns = 3;                        // 지속 턴 수 (기본 3)
    [Range(1, 500)] public int utilityPercent = 120;    // 120 = +20%
    
    [Header("VFX / Prefabs (선택)")]
    public GameObject effectPrefab;
}
