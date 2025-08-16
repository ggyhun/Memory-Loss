using UnityEngine;

public class HighlightTile : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Color baseColor = new Color(1, 1, 1, 0.4f);
    public Color offsetColor = new Color(1, 1, 1, 0.8f);

    private Vector3Int cellPos;
    private System.Action<Vector3Int> onClickCallback;

    public void Setup(Vector3Int cell, System.Action<Vector3Int> clickCallback)
    {
        cellPos = cell;
        onClickCallback = clickCallback;
        spriteRenderer.color = baseColor;
    }

    void OnMouseEnter()
    {
        spriteRenderer.color = offsetColor;
    }

    void OnMouseExit()
    {
        spriteRenderer.color = baseColor;
    }

    void OnMouseDown()
    {
        onClickCallback?.Invoke(cellPos);
    }
}