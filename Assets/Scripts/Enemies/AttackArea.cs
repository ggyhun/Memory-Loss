using UnityEngine;
using System.Collections;

public class EnemyAttackArea : MonoBehaviour
{
    private bool canAttack = false; // 공격 가능 여부
    public SpriteRenderer spriteRenderer;

    [Header("Editor Settings")]
    public bool isVisible = false;

    [Header("Flash Settings")]
    // RGBA (255, 81, 81, 38) → (1.0, 0.3176, 0.3176, 0.1490)
    public Color flashColor = new Color(1f, 81f/255f, 81f/255f, 38f/255f);
    public float flashDuration = 0.5f;
    public bool revertToTransparent = false; // 끝나면 완전 투명으로

    private Color _originalColor;
    private Coroutine _flashRoutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on EnemyAttackArea.");
            return;
        }

        _originalColor = spriteRenderer.color;

        var c = spriteRenderer.color;
        spriteRenderer.color = new Color(c.r, c.g, c.b, 0f);
    }

    private void OnDisable()
    {
        // 중간에 비활성화되면 코루틴 종료 및 색상 정리
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        if (spriteRenderer != null)
        {
            if (revertToTransparent)
                spriteRenderer.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, 0f);
            else
                spriteRenderer.color = _originalColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"EnemyAttackArea: {other.name} entered the attack area.");
        if (other.CompareTag("Player"))
        {
            canAttack = true;
            Debug.Log("Enemy can attack the player.");

            // 들어오면 깜빡 효과
            StartFlash();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canAttack = false;
            Debug.Log("Enemy can no longer attack the player.");
        }
    }

    public bool CanAttack() => canAttack;

    // ===== Flash API =====
    public void StartFlash()
    {
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(CoFlashOnce());
    }

    private IEnumerator CoFlashOnce()
    {
        if (spriteRenderer == null) yield break;

        // 즉시 지정 색상으로
        spriteRenderer.color = flashColor;

        // 0.3초 대기 (애니메이션과 '동시에' 보임)
        yield return new WaitForSeconds(flashDuration);

        // 종료 시 투명/원래 색으로 복구
        if (revertToTransparent)
            spriteRenderer.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, 0f);
        else
            spriteRenderer.color = _originalColor;

        _flashRoutine = null;
    }
}
