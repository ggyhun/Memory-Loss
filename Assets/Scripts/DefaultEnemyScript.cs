using Unity.VisualScripting;
using UnityEngine;

public class DefaultEnemyScript : MonoBehaviour
{
    void Awake()
    {
        EnemyManager enemyManager = GameObject.Find("EnemyManager").GetComponent<EnemyManager>();
        enemyManager.RegisterEnemy(gameObject);
    }

    void Update()
    {
        
    }
}
