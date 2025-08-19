using UnityEngine;

public class SpellHighlighter : MonoBehaviour
{
    private SpriteRenderer rend;

    public Color baseColor = new Color(0.8f, 0.2f, 0.2f, 0.6f); // 빨강 투명

    private void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
        rend.color = baseColor;
        Hide();
    }

    public void Show()
    {
        rend.enabled = true;
    }

    public void Hide()
    {
        rend.enabled = false;
    }
}
