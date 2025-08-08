using Unity.VisualScripting;
using UnityEngine;

public class DefaultEnemyScript : MonoBehaviour
{
    void Awake()
    {
        // 현재 DefaultEnemyScript를 EnemyManager에 등록하는 코드, 하지만 현재 Enemy 스크립트를 사용할 예정
        /*EnemyManager enemyManager = GameObject.Find("EnemyManager").GetComponent<EnemyManager>();
        enemyManager.RegisterEnemy(gameObject);*/
    }

    void Update()
    {
        
    }
}
