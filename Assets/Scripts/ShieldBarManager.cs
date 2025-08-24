using UnityEngine;

public class ShieldBarManager : MonoBehaviour
{
    private Stats stats;
    private Vector3 originalScale;
    private SpriteRenderer ShieldBarRenderer;

    [Header("Shield Bar Setting")]
    public GameObject shieldBar;
    
    private void Start()
    {
        stats = GetComponentInParent<Stats>();
        ShieldBarRenderer = shieldBar.GetComponent<SpriteRenderer>();
        ShieldBarRenderer.color = new Color(1f, 1f, 1f, 0.7f);
    }

    private void Update()
    {
        shieldBar.transform.localScale = new Vector3((float)stats.shieldHp / stats.maxHp, 0.4f, 1);
    }
}