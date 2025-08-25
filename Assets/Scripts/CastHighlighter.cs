using UnityEngine;

public class CastHighlighter : MonoBehaviour
{
    public HighlightManager manager;
    public Vector3Int castCell;
    private SpriteRenderer rend;

    public Color baseColor  = new Color(1,1,1,0.4f);
    public Color hoverColor = new Color(1,1,1,0.8f);

    void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
        rend.color = baseColor;
    }

    public void SetHover(bool on)
    {
        if (rend) rend.color = on ? hoverColor : baseColor;
    }
}
