using UnityEngine;
using System.Collections.Generic;

public class Scroll : MonoBehaviour
{
    [Header("Spell")]
    public SpellData spellData;
    
    [Header("Optional")]
    public bool isRandom = false; // 랜덤 스크롤 여부
    public List<SpellData> spellDataList; // 여러 개의 스크롤 데이터 지원 (예: 랜덤 선택)
    public GridManager gridManager; // 비워두면 자동 탐색

    private SpriteRenderer spriteRenderer;
    
    private void Awake()
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (!isRandom && spellData == null)
            Debug.LogWarning($"{name}: spellData가 비어있습니다. 스크롤 효과가 없을 수 있습니다.");
        if (isRandom && (spellDataList == null || spellDataList.Count == 0))
        {
            Debug.LogWarning($"{name}: spellDataList가 비어있습니다. 랜덤 스크롤 효과가 없을 수 있습니다.");
            isRandom = false; // 랜덤 스크롤이 비활성화됨
        }

        if (isRandom)
        {
            // 랜덤 스크롤 데이터 선택
            int randomIndex = Random.Range(0, spellDataList.Count);
            spellData = spellDataList[randomIndex];
        }
        SetIcon();
    }

    public void TryPickup(GameObject other)
    {
        // 플레이어가 아니면 무시
        if (other == null) return;
        if (other.CompareTag("Player") == false)
        {
            Debug.Log($"{name}: {other.name} is not a player. Ignoring pickup.");
            return;
        }
        
        
        var player = other.GetComponent<PlayerController>();
        if (player == null) return;

        // 배움
        if (spellData != null)
        {
            player.LearnSpell(spellData);
            Debug.Log($"Player learned spell: {spellData.spellName}");
        }

        // 스크롤 제거
        Destroy(gameObject);
    }

    private void SetIcon()
    {
        // 속성에 따라 스프라이트 설정
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null && spellData != null)
        {
            spriteRenderer.sprite = spellData.icon;
        }
        else
        {
            Debug.LogWarning($"{name}: SpriteRenderer 또는 spellData가 비어있습니다. 아이콘을 설정할 수 없습니다.");
        }
    }
}
