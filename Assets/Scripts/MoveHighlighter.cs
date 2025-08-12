using UnityEngine;

public class MoveHighlighter : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Color baseColor = new Color(1, 1, 1, 0.4f);
    public Color offsetColor = new Color(1, 1, 1, 0.8f);
    public HighlightManager highlightManager;

    void Awake()
    {
        // 스프라이트 렌더러 초기화 : 연결되어있지 않다면 연결
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (highlightManager == null)
        {
            highlightManager = FindFirstObjectByType<HighlightManager>();
        }
        
        // 기본 색상 설정
        spriteRenderer.color = baseColor;
    }
    
    // 하이라이트 투명도 조정
    void OnMouseEnter()
    {
        Debug.Log("하이라이트 활성화 : " + transform.position);
        spriteRenderer.color = offsetColor;
    }

    // 하이라이트 투명도 조정
    void OnMouseExit()
    {
        spriteRenderer.color = baseColor;
    }

    void OnMouseDown()
    {
        Debug.Log("하이라이트 클릭 : " + transform.position);
        highlightManager.HandleMoveHighlighterClick(transform.position);
    }
}