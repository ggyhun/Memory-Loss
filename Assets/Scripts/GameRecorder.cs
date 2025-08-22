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

    public int GetTotalKillCount()
    {
        return TotalKillCount;
    }

    public int GetReachedFloor()
    {
        return ReachedFloor;
    }

    public bool GetIsGameCleared()
    {
        return IsGameCleared;
    }

    public void AddKillCount()
    {
        TotalKillCount++;
    }

    public void AddReachedFloor()
    {
        ReachedFloor++;
    }
}
