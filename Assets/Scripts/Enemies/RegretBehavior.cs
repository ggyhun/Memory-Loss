using System;
using UnityEngine;

[RequireComponent(typeof(EnemyMover))]
[RequireComponent(typeof(Stats))]
public class RegertBehavior : EnemyBehavior
{
    [Header("Regret Behavior Settings")]
    public int impliedDamagePercentage = 20;
    public Animator animator;
    
    private EnemyMover mover;
    private Transform player;       // 필드만 사용
    private Stats playerStats;
    private EnemyAttackArea attackArea;
    private GridManager grid;
    private Stats myStats;

    private bool _canMove = true;
    private int _previousHealth;
    
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
    }

    public override void Act(Enemy enemy)
    {
        if (mover == null || player == null || grid == null) return;
        if (!myStats.CanAct) return;

        if (!_canMove)
        {
            _canMove = true;
            _previousHealth = myStats.currentHp;
            return;
        }

        // 체력 변화 체크
        int offset = _previousHealth - myStats.currentHp; // 과거가 더 체력이 많다면, 즉, 피해를 입었다면
        if (offset > 0)
        {
            int impliedDamage = Mathf.CeilToInt(offset * (impliedDamagePercentage / 100f));
            playerStats?.TakeDamage(impliedDamage);
            Debug.Log($"{name} inflicted {impliedDamage} implied damage to the player.");
        }

        _previousHealth = myStats.currentHp;
        
        animator.SetTrigger("Move");
        
        mover.TryStepTowardTarget(enemy.gameObject, player.gameObject);
        mover.TryStepTowardTarget(enemy.gameObject, player.gameObject);
        _canMove = false;
    }
}
