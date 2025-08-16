using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private EnemyManager enemyManager;
    private EnemyBehavior behavior; // ✅ 연결되는 Behavior
    
    private void Start()
    {
        // EnemyManager 등록
        enemyManager = FindObjectOfType<EnemyManager>();
        enemyManager.RegisterEnemy(this);

        // 프리팹에 붙은 EnemyBehavior 가져오기
        behavior = GetComponent<EnemyBehavior>();
        if (behavior == null)
        {
            Debug.LogWarning($"{name}에 EnemyBehavior가 없습니다!");
        }
    }

    public void StartTurnAction()
    {
        StartCoroutine(TakeTurnRoutine());
    }

    private IEnumerator TakeTurnRoutine()
    {
        // EnemyBehavior가 있다면 실행
        if (behavior != null)
        {
            behavior.Act(this);
        }

        // 행동 후 약간의 텀 (예시)
        yield return new WaitForSeconds(0.3f);

        // EnemyManager에 보고
        enemyManager.ReportEnemyDone();
    }

    private void OnDestroy()
    {
        if (enemyManager != null)
            enemyManager.UnregisterEnemy(this);
    }
}
