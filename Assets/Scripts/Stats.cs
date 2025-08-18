using UnityEngine;

public enum ElementType { Normal, Ice, Fire, Water }
public enum StatusType  { Frozen, Burning, Wet }

public class Stats : MonoBehaviour
{
    [Header("HP")]
    public int maxHp = 100;
    public int currentHp;

    // --- 이하 상태이상 관련 필드 (앞서 작성한 것과 동일) ---
    [Header("Status Effects")]
    [SerializeField] private int frozenTurns;   
    [SerializeField] private int burningTurns;  
    [SerializeField] private int wetTurns;      

    [Header("Status Defaults")]
    public int frozenDefaultTurns  = 2;
    public int burningDefaultTurns = 3;
    public int wetDefaultTurns     = 3;
    public int burningDotDamage    = 5;
    
    [Header("Status Enhancements")]
    public int frozenEnhancementTurns  = 0; // 빙결 지속시간 증가
    public int burningEnhancementTurns = 0; // 화염 지속시간 증가
    public int wetEnhancementTurns     = 0; // 젖음 지속시간 증가
    public int frozenEnhancementAmount = 100; // 빙결 강화 효과 (% 단위)
    public int burningEnhancementAmount= 100; // 화염 강화 효과 (% 단위)
    public int wetEnhancementAmount    = 100; // 젖음 강화 효과 (% 단위)
    
    public bool IsFrozen  => frozenTurns  > 0;
    public bool IsBurning => burningTurns > 0;
    public bool IsWet     => wetTurns     > 0;
    public bool CanAct    => !IsFrozen;

    private void Awake()
    {
        currentHp = Mathf.Clamp(currentHp == 0 ? maxHp : currentHp, 0, maxHp);
    }

    // ========= HP =========
    public void TakeDamage(int amount, ElementType element = ElementType.Normal)
    {
        if (amount > 0)
        {
            currentHp = Mathf.Max(0, currentHp - amount);
            // Debug.Log($"{name} took {amount} {element} dmg. HP: {currentHp}/{maxHp}");

            if (currentHp <= 0)
            {
                Die();
                return;
            }
        }

        // 원소 판정
        if (element != ElementType.Normal)
        {
            TryApplyElement(element);
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHp = Mathf.Min(maxHp, currentHp + amount);
    }

    private void Die()
    {
        // Debug.Log($"{name} died.");
        Destroy(gameObject);
    }

    // ========= Turn Hooks =========
    /// <summary>자신의 턴 "시작 시" 호출: 화염 DOT 적용 등</summary>
    public void OnTurnStart()
    {
        if (IsBurning)
        {
            TakeDamage(burningDotDamage); // 표: 턴 시작 시 5 데미지
        }
    }

    /// <summary>자신의 턴 "종료 시" 호출: 지속시간 1틱 소모</summary>
    public void OnTurnEnd()
    {
        if (frozenTurns  > 0) frozenTurns--;
        if (burningTurns > 0) burningTurns--;
        if (wetTurns     > 0) wetTurns--;

        // 턴 종료 후 만료 상태 정리 로깅 원하면 여기 추가
    }

    // ========= Status Apply APIs =========
    /// <summary>원소 피격에 따른 상태 적용(표 규칙 반영)</summary>
    public bool TryApplyElement(ElementType element)
    {
        switch (element)
        {
            case ElementType.Ice:
                return TryApplyStatus(StatusType.Frozen);
            case ElementType.Fire:
                return TryApplyStatus(StatusType.Burning);
            case ElementType.Water:
                return TryApplyStatus(StatusType.Wet);
            default:
                return false;
        }
    }

    /// <summary>상태이상 직접 적용 (상호배제/해제 규칙 포함)</summary>
    public bool TryApplyStatus(StatusType status)
    {
        switch (status)
        {
            case StatusType.Frozen:
                // 표: 빙결은 "젖음 상태의 몬스터에게만" 적용
                if (!IsWet) return false;

                // 빙결 적용
                frozenTurns = frozenDefaultTurns;
                // 표에 Wet 해제 언급 없음 → 유지 (원하면 wetTurns=0 해도 됨)
                // 화염과의 관계: 화염이 있으면? 표에는 언급X → 그대로 둠
                // Debug.Log($"{name} is Frozen for {frozenTurns} turns.");
                return true;

            case StatusType.Burning:
                // 표: 빙결 상태에게는 적용되지 않으며, 즉시 빙결 해제
                if (IsFrozen)
                {
                    frozenTurns = 0; // 빙결 해제
                    return false;    // 화염 미적용
                }
                // 나머지에게는 적용
                burningTurns = burningDefaultTurns;
                // 젖음과의 상호작용: 표에는 '화염 적용 시 Wet 해제'가 명시되어 있지 않음 → 유지
                // Debug.Log($"{name} is Burning for {burningTurns} turns.");
                return true;

            case StatusType.Wet:
                // 표: 화염 상태인 대상에게는 적용되지 않으며, 즉시 화염 해제
                if (IsBurning)
                {
                    burningTurns = 0; // 화염 해제
                    return false;     // 젖음 미적용
                }
                // 적용
                wetTurns = wetDefaultTurns;
                // Debug.Log($"{name} is Wet for {wetTurns} turns.");
                return true;
        }
        return false;
    }

    // ========= Convenience =========
    public void ClearStatus(StatusType status)
    {
        switch (status)
        {
            case StatusType.Frozen:  frozenTurns = 0; break;
            case StatusType.Burning: burningTurns = 0; break;
            case StatusType.Wet:     wetTurns = 0; break;
        }
    }

    public void ClearAllStatuses()
    {
        frozenTurns = 0;
        burningTurns = 0;
        wetTurns = 0;
    }
}
