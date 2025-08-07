using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private EnemyManager enemyManager;
    
    private void Start()
    {
        // EnemyManager를 찾아서 등록
        enemyManager = FindObjectOfType<EnemyManager>();
        enemyManager.RegisterEnemy(this);
    }

    public void StartTurnAction()
    {
        // 적의 턴 행동을 시작
        StartCoroutine(TakeTurnRoutine());
    }

    private IEnumerator TakeTurnRoutine()
    {
        // Todo: 적의 행동 로직 구현
        // 예시로 0.3초 대기 후 턴 완료
        yield return new WaitForSeconds(0.3f);

        // EnemyManager에 보고
        enemyManager.ReportEnemyDone();
    }

    private void OnDestroy()
    {
        // EnemyManager에서 제거
        // 메모리 누수 방지를 위해 필요
        if (enemyManager != null)
            enemyManager.UnregisterEnemy(this);
    }
}
