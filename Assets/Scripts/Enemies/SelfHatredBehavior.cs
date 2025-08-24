using System;
using UnityEngine;

public class SelfHatredBehavior : EnemyBehavior
{
    [Header("Self-Hatred Behavior Settings")]
    public int threshold1Percent = 50; // Hp 100% -> 50%
    public int threshold2Percent = 25; // Hp 50% -> 25%
    private bool phase2Entered = false;
    private bool phase3Entered = false;
    private int previousHp;
    
    [Header("Self-Hatred Attack Settings")]
    public int attackDecisionRange = 1; 
    public int skill1Damage = 8;
    public int skill2Damage = 12;
    public int skill3Damage = 8;
    public int phase = 1; // 1 -> 2 -> 3 -> 4
    private int currentPhase = 1;
    private int skill1amplification = 100; // phase 2 부터 + 50 * (phase - 1)
    private int skill2amplification = 100; // phase 2 부터 + 50 * (phase - 1)
    private int skill3amplification = 100; // phase 2 부터 + 50 * (phase - 1)
    public GameObject skill1AttackAreaPrefab;
    public GameObject skill2AttackAreaPrefab;
    public GameObject skill3AttackAreaPrefab;
    
    private EnemyMover mover;
    private Transform player;       // 필드만 사용
    private Stats playerStats;
    private EnemyAttackArea skill1AttackArea;
    private EnemyAttackArea skill2AttackArea;
    private EnemyAttackArea skill3AttackArea;
    private GridManager grid;
    private Stats myStats;

    private int nextSkill;

    private void Awake()
    {
        mover    = GetComponent<EnemyMover>();
        myStats  = GetComponent<Stats>();
        grid     = FindFirstObjectByType<GridManager>();

        var pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) {
            player      = pObj.transform;
            playerStats = pObj.GetComponent<Stats>();
        } else {
            Debug.LogWarning($"{name}: Player not found.");
        }

        if (skill1AttackAreaPrefab == null || skill2AttackAreaPrefab == null || skill3AttackAreaPrefab == null)
        {
            Debug.LogWarning($"{name}: One or more skill attack area prefabs are not assigned.");
            return;
        }
        skill1AttackArea = skill1AttackAreaPrefab.GetComponent<EnemyAttackArea>();
        skill2AttackArea = skill2AttackAreaPrefab.GetComponent<EnemyAttackArea>();
        skill3AttackArea = skill3AttackAreaPrefab.GetComponent<EnemyAttackArea>();
    }

    public override void Act(Enemy enemy)
    {
        if (mover == null || player == null || grid == null) return;
        if (!myStats.CanAct) return;
        
        ActionDecision(enemy);
        
        previousHp = myStats.currentHp; // 턴 종료 시 체력 업데이트
    }

    private void ActionDecision(Enemy enemy)
    {
        var enemyCell  = grid.WorldToCell(enemy.transform.position);
        var playerCell = grid.WorldToCell(player.position);
        var d = playerCell - enemyCell;
        int dist2 = Math.Max(Math.Abs(d.x), Math.Abs(d.y));
        
        int updatedPhase = UpdatePhase();
        if (updatedPhase != currentPhase)
        {
            currentPhase = updatedPhase;
            // 스킬 3 사용
            UseSkill(3, d);
        }
        else if (dist2 <= attackDecisionRange)
        {
            // 스킬 1과 2중 랜덤 하나 사용
            nextSkill = UnityEngine.Random.Range(1, 3); // 1 또는 2
            UseSkill(nextSkill, d);
        }
        else
        {
            // 이동
            mover.TryStepTowardTarget(enemy.gameObject, player.gameObject);
        }
    }
    
    private void UseSkill(int skillNumber, Vector3Int direction = default)
    {
        int damage = 0;
        switch (skillNumber)
        {
            case 1:
                damage = skill1Damage * skill1amplification / 100;
                if (skill1AttackArea != null && skill1AttackArea.CanAttack())
                {
                    skill1AttackArea.StartFlash();
                    playerStats.TakeDamage(damage, ElementEffectType.Ice);
                }
                break;
            case 2:
                damage = skill2Damage * skill2amplification / 100;
                if (skill2AttackArea != null && skill2AttackArea.CanAttack())
                {
                    skill2AttackArea.StartFlash();
                    playerStats.TakeDamage(damage, ElementEffectType.Water);
                }
                break;
            case 3:
                damage = skill3Damage * skill3amplification / 100;
                playerStats.TakeDamage(damage, ElementEffectType.Fire);
                break;
            default:
                Debug.LogWarning("Invalid skill number");
                return;
        }
        mover.enemyAnimator.PlayAttack(direction);
        Debug.Log($"{name} used Skill {skillNumber} dealing {damage} damage!");
    }

    private int UpdatePhase()
    {
        if (myStats == null) return phase;
        
        int currentHp = myStats.currentHp;
        if (previousHp == 0) previousHp = currentHp; // 첫 호출 시 초기화

        int maxHp = myStats.maxHp;
        int threshold1 = maxHp * threshold1Percent / 100;
        int threshold2 = maxHp * threshold2Percent / 100;

        if (!phase2Entered && currentHp <= threshold1 && previousHp > threshold1)
        {
            phase2Entered = true;
            phase = 2;
            skill1amplification += 50 * (phase - 1);
            skill2amplification += 50 * (phase - 1);
            Debug.Log($"{name} has entered Phase 2!");
        }
        else if (!phase3Entered && currentHp <= threshold2 && previousHp > threshold2)
        {
            phase3Entered = true;
            phase = 3;
            skill1amplification += 50 * (phase - 1);
            skill2amplification += 50 * (phase - 1);
            skill3amplification += 50 * (phase - 2);
            Debug.Log($"{name} has entered Phase 3!");
        }

        previousHp = currentHp; // 현재 체력을 이전 체력으로 업데이트
        return phase;
    }
}
