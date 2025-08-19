using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    public TextMeshProUGUI FloorText;
    public TextMeshProUGUI MonsterText;
    void Awake()
    {
        UpdateTextUI();
    }

    void UpdateTextUI()
    {
        FloorText.text = 1.ToString() + "층";
        MonsterText.text = 1.ToString();
    }
    public void OnClickRetryButton()
    {

    }
    public void OnClickMainMenuButton()
    {
        
    }
}
