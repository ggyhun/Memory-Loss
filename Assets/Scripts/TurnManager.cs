using UnityEngine;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public enum TurnState { PlayerTurn, SystemTurn }
    public TurnState CurrentTurn { get; private set; }

    private int _totalPlayerActors;
    private int _finishedPlayerActorCount;

    [SerializeField] private EnemyManager enemyManager;

    public void RegisterActor() => _totalPlayerActors++;
    public void UnregisterActor() => _totalPlayerActors--;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        CurrentTurn = TurnState.PlayerTurn;
        
        _totalPlayerActors = 0;
        _finishedPlayerActorCount = 0;
        
        if (enemyManager == null)
        {
            enemyManager = FindFirstObjectByType<EnemyManager>();
            if (enemyManager == null)
            {
                Debug.LogError("EnemyManager not found in the scene.");
            }
        }
    }

    public bool IsPlayerTurn()
    {
        return CurrentTurn == TurnState.PlayerTurn;
    }
    
    // PlayerController에서 호출
    public void EndPlayerTurn()
    {
        _finishedPlayerActorCount = 0;
        CurrentTurn = TurnState.SystemTurn;
        enemyManager.StartEnemyTurn();
    }
    
    public void StartPlayerTurn()
    {
        CurrentTurn = TurnState.PlayerTurn;
        
        FindFirstObjectByType<PlayerController>().StartTurn();
        
        _finishedPlayerActorCount = 0;
    }

    public void PlayerActorReportDone()
    {
        _finishedPlayerActorCount++;
        if (_finishedPlayerActorCount >= _totalPlayerActors)
        {
            Debug.Log("플레이어 턴 종료");
            EndPlayerTurn();
        }
    }
}