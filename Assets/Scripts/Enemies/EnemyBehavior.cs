using UnityEngine;

public abstract class EnemyBehavior : MonoBehaviour
{
    // Enemy가 전달되어 행동을 수행할 수 있도록
    public abstract void Act(Enemy enemy);
}
