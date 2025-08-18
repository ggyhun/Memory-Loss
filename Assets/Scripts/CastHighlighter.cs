using UnityEngine;

public class CastHighlighter : MonoBehaviour
{
    public HighlightManager manager;
    public Vector3Int castCell;
    private SpriteRenderer rend;

    public Color baseColor = new Color(1,1,1,0.4f);
    public Color hoverColor = new Color(1,1,1,0.8f);

    private void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
        rend.color = baseColor;
    }

    private void OnMouseEnter()
    {
        rend.color = hoverColor;
        manager.ShowSpellHighlights(castCell);
    }
    
    private void OnMouseExit()
    {
        rend.color = baseColor;
        manager.ClearSpellHighlights();
    }

    private void OnMouseDown()
    {
        Debug.Log("Cast!");
        manager.ConfirmCast(castCell);   // ✅ 시전 확정
    }
}
