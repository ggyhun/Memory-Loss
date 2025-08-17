using UnityEngine;

public class Scroll : MonoBehaviour
{
    [Header("Spell")]
    public SpellData spellData;

    [Header("Optional")]
    public GridManager gridManager; // 비워두면 자동 탐색

    private void Awake()
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (spellData == null)
            Debug.LogWarning($"{name}: spellData가 비어있습니다. 스크롤 효과가 없을 수 있습니다.");
    }

    // ---- 2D 물리용 ----
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryPickup(other.gameObject);
    }

    // ---- 3D 물리용 ----
    private void OnTriggerEnter(Collider other)
    {
        TryPickup(other.gameObject);
    }

    private void TryPickup(GameObject other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player == null) return;

        // 배움
        if (spellData != null)
        {
            player.LearnSpell(spellData);
            Debug.Log($"Player learned spell: {spellData.spellName}");
        }

        // 타일 점유 해제
        if (gridManager != null)
        {
            var cell = gridManager.WorldToCell(transform.position);
            gridManager.ClearOccupant(cell);
        }

        // 스크롤 제거
        Destroy(gameObject);
    }
}
