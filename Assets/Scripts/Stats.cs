using UnityEngine;

public class Stats : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth;
    public int currnetHealth;

    public void TakeDamage(int damange)
    {
        currnetHealth -= damange;
        if (currnetHealth <= 0)
        {
            currnetHealth = 0;
            Die();
        }
    }

    public void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        // 여기서 적 처치 로직을 추가할 수 있습니다.
        // 예: 적 제거, 애니메이션 재생, 점수 증가 등
        Destroy(gameObject);
    }
}
