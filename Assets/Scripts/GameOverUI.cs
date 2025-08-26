using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    public TextMeshProUGUI FloorText;
    public TextMeshProUGUI MonsterText;
    public TextMeshProUGUI UITitleText;
    void Awake()
    {
        UpdateTextUI();
    }

    void UpdateTextUI()
    {
        if (GameRecorder.Instance.GetIsGameCleared())
        {
            UITitleText.text = "GAME CLEAR";
        }
        else
        {
            UITitleText.text = "GAME OVER";
            AudioManager.Instance.PlayBGM(5);
        }
        FloorText.text = GameRecorder.Instance.GetReachedFloor().ToString() + "층";
        MonsterText.text = GameRecorder.Instance.GetTotalKillCount().ToString();
    }
    public void OnClickRetryButton()
    {
        SceneLoader.Instance.LoadScene("PlayScene");
    }
    public void OnClickMainMenuButton()
    {
        SceneLoader.Instance.LoadScene("Start");
    }
}
