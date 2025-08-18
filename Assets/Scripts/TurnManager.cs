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
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player GameObject not found. Cannot end turn.");
            return;
        }
        var playerStats = player.GetComponent<Stats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats component not found on Player GameObject. Cannot end turn.");
            return;
        }
        playerStats.OnTurnEnd();
        _finishedPlayerActorCount = 0;
        CurrentTurn = TurnState.SystemTurn;
        enemyManager.StartEnemyTurn();
    }
    
    public void StartPlayerTurn()
    {
        HighlightManager highlightManager = FindFirstObjectByType<HighlightManager>();
        highlightManager.ClearSpellHighlights();
        
        CurrentTurn = TurnState.PlayerTurn;
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player GameObject not found. Cannot end turn.");
            return;
        }
        var playerStats = player.GetComponent<Stats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats component not found on Player GameObject. Cannot end turn.");
            return;
        }
        playerStats.OnTurnStart();
        FindFirstObjectByType<PlayerController>().StartTurn();
        
        _finishedPlayerActorCount = 0;
    }
}