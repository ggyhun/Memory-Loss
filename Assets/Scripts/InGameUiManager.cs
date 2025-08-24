using System;
using UnityEngine;
using TMPro;

public class InGameUiManager : MonoBehaviour
{
    public static InGameUiManager Instance;
    
    public TextMeshProUGUI floorText;
    public TextMeshProUGUI enemyKillCountText;
    public EnemyManager enemyManager;
    
    public int currentFloor = 0;
    public int enemyKillCount = -1;
    public int goalKillCount = -1;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    private void Start()
    {
        UpdateFloorText();
        UpdateEnemyKillCountText();
    }
    
    private void Update()
    {
        enemyManager.GetKillCounts(out int current, out int goal);
        enemyKillCount = current;
        goalKillCount = goal;
        UpdateEnemyKillCountText();
        UpdateFloorText();
    }
    
    public void UpdateFloorText()
    {
        floorText.text = $"{currentFloor}F";
    }
    
    public void UpdateEnemyKillCountText()
    {
        enemyKillCountText.text = $"{enemyKillCount}/{goalKillCount}";
    }
}
