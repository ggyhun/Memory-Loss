using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private string itemDescription; // 이 슬롯의 아이템 설명
    [SerializeField] private Tooltip tooltip; // Tooltip 스크립트 참조
    private GameObject canvas;

    private void Start()
    {
        canvas = GameObject.Find("Canvas");
        tooltip = canvas.transform.GetChild(0).GetComponent<Tooltip>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.ShowTooltip(itemDescription, new Vector2(transform.position.x + 30, transform.position.y));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.HideTooltip();
    }
}
