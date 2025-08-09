using UnityEngine;
using UnityEngine.Events;

public class StatsSystem : MonoBehaviour
{
    [Header("기본 스탯")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("이벤트")]
    public UnityEvent onDeath;
    public UnityEvent<int> onHealthChanged;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// 데미지 받기
    /// </summary>
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"{gameObject.name}이(가) {amount} 데미지를 받음. 남은 체력: {currentHealth}");

        onHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 체력 회복
    /// </summary>
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"{gameObject.name}이(가) {amount} 회복. 현재 체력: {currentHealth}");

        onHealthChanged?.Invoke(currentHealth);
    }

    /// <summary>
    /// 사망 처리
    /// </summary>
    private void Die()
    {
        Debug.Log($"{gameObject.name} 사망!");
        onDeath?.Invoke();
        Destroy(gameObject);
    }
}
