using UnityEngine;

public class GameRecorder : MonoBehaviour
{
    public static GameRecorder Instance { get; private set; }
    public int TotalKillCount = 0;
    public int ReachedFloor = 0;
    public bool IsGameCleared = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void ResetAllRecorded()
    {
        MapGenerator.Instance.currentLevelIndex = 0;
        TotalKillCount = 0;
        ReachedFloor = 0;
        IsGameCleared = false;
    }
}
