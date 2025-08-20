using UnityEngine;

public class EnemyAttackArea : MonoBehaviour
{
    private bool canAttack = false; // 공격 가능 여부
    public SpriteRenderer spriteRenderer;
    
    [Header("Editor Settings")]
    public bool isVisible = true;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on EnemyAttackArea.");
        }
        if (!isVisible) spriteRenderer.color = new Color(0, 0, 0, 0f);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"EnemyAttackArea: {other.name} entered the attack area.");
        if (other.CompareTag("Player"))
        {
            canAttack = true; // 플레이어가 공격 범위에 들어오면 공격 가능
            Debug.Log("Enemy can attack the player.");
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canAttack = false; // 플레이어가 공격 범위를 벗어나면 공격 불가능
            Debug.Log("Enemy can no longer attack the player.");
        }
    }
    
    public bool CanAttack()
    {
        return canAttack;
    }
}