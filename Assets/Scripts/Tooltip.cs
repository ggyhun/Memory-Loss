using UnityEngine;
using TMPro;
using System.Linq.Expressions;

public class Tooltip : MonoBehaviour
{
    [SerializeField] private TMP_Text descriptionText; // ���� ���� �ؽ�Ʈ

    void Start()
    {
        HideTooltip(); // ������ �� ����
    }

    public void ShowTooltip(string description, Vector2 position)
    {
        this.gameObject.SetActive(true);
        transform.position = position;
        descriptionText.text = description;
    }

    public void HideTooltip()
    {
        this.gameObject.SetActive(false);
    }
}
