using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private EnemyManager enemyManager;
    private EnemyBehavior behavior;
    private Stats stats;

    private bool reportedThisTurn = false; // ✅ 중복 방지

    private void Start()
    {
        enemyManager = FindObjectOfType<EnemyManager>();
        enemyManager.RegisterEnemy(this);

        behavior = GetComponent<EnemyBehavior>();
        stats    = GetComponent<Stats>();
        if (stats != null) stats.OnDied += HandleDied;
    }

    private void OnDestroy()
    {
        if (stats != null) stats.OnDied -= HandleDied;
        if (enemyManager != null) enemyManager.UnregisterEnemy(this);
    }

    public void StartTurnAction()
    {
        reportedThisTurn = false; // ✅ 턴 시작 시 리셋
        StartCoroutine(TakeTurnRoutine());
    }

    private IEnumerator TakeTurnRoutine()
    {
        if (behavior != null) behavior.Act(this);
        yield return new WaitForSeconds(0.3f);

        if (!reportedThisTurn)
        {
            reportedThisTurn = true;
            enemyManager.ReportEnemyDone();
        }
    }

    private void HandleDied()
    {
        enemyManager.ReportEnemyKilled(this);

        if (!reportedThisTurn)
        {
            reportedThisTurn = true;
            enemyManager.ReportEnemyDone();
        }
    }
}
