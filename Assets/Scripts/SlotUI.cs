using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI count;

    public void Clear() {
        if (icon) { icon.enabled = false; icon.sprite = null; }
        if (count) count.text = "";
    }

    public void Set(Sprite sprite, int amount = 1) {
        if (!sprite) { Clear(); return; }
        icon.enabled = true;
        icon.sprite = sprite;
        icon.preserveAspect = true;
        if (count) count.text = (amount >= 1) ? amount.ToString() : "0";
    }
}
