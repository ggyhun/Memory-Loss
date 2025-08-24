using System;
using UnityEngine;

public enum HpBarType { Player, Enemy }

public class HpBarManager : MonoBehaviour
{
    private Stats stats;
    private Vector3 originalScale;
    private SpriteRenderer hpBarRenderer;

    [Header("HP Bar Setting")]
    public HpBarType hpBarType = HpBarType.Enemy;
    public GameObject hpBar;
    
    private void Start()
    {
        stats = GetComponentInParent<Stats>();
        hpBarRenderer = hpBar.GetComponent<SpriteRenderer>();
        hpBarRenderer.color = Color.red;
    }
    
    private void Update()
    {
        UpdateHPBar();
    }
    
    public void UpdateHPBar()
    {
        if (stats == null) return;
        float healthPercent = (float)stats.currentHp / stats.maxHp;
        hpBar.transform.localScale = new Vector3(healthPercent, 0.4f, 1);

        if (hpBarType == HpBarType.Enemy) return;
        
        // 체력 기본 초록색
        if (healthPercent > 0.75f)
        {
            hpBarRenderer.color = Color.green;
        }
        else if (healthPercent > 0.5f)
        {
            hpBarRenderer.color = Color.yellow;
        }
        else if (healthPercent > 0.25f)
        {
            // 주황색 FF7F00
            hpBarRenderer.color = new Color(1f, 0.5f, 0f);
        }
        else
        {
            hpBarRenderer.color = Color.red;
        }
    }
}
