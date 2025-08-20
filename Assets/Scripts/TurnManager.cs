using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public enum TurnState { PlayerTurn, SystemTurn }
    public TurnState CurrentTurn { get; private set; }

    [Header("Swap Delay")]
    public float turnSwapDelay = 1f;          // 1초 대기
    public bool useUnscaledTime = true;       // timeScale=0이어도 대기하도록

    private int _totalPlayerActors;
    private int _finishedPlayerActorCount;

    [SerializeField] private EnemyManager enemyManager;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        CurrentTurn = TurnState.PlayerTurn;
        _totalPlayerActors = 0;
        _finishedPlayerActorCount = 0;

        if (enemyManager == null)
        {
            enemyManager = FindFirstObjectByType<EnemyManager>();
            if (enemyManager == null) Debug.LogError("EnemyManager not found in the scene.");
        }
    }

    public bool IsPlayerTurn() => CurrentTurn == TurnState.PlayerTurn;

    // ===== 전환 코루틴 헬퍼 =====
    private IEnumerator DelaySwap()
    {
        if (turnSwapDelay <= 0f) yield break;
        if (useUnscaledTime) yield return new WaitForSecondsRealtime(turnSwapDelay);
        else                 yield return new WaitForSeconds(turnSwapDelay);
    }

    // ===== 플레이어 → 시스템 =====
    public void EndPlayerTurn() => StartCoroutine(CoEndPlayerTurn());

    private IEnumerator CoEndPlayerTurn()
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null) { Debug.LogError("Player not found."); yield break; }
        var playerStats = player.GetComponent<Stats>();
        if (playerStats == null) { Debug.LogError("Stats not found on Player."); yield break; }

        // 플레이어 턴 종료 훅
        playerStats.OnTurnEnd();

        _finishedPlayerActorCount = 0;
        CurrentTurn = TurnState.SystemTurn;

        // 전환 딜레이
        yield return DelaySwap();

        // 적 턴 시작
        enemyManager.StartEnemyTurn();
    }

    // ===== 시스템 → 플레이어 =====
    public void StartPlayerTurn() => StartCoroutine(CoStartPlayerTurn());

    private IEnumerator CoStartPlayerTurn()
    {
        // 전환 딜레이
        yield return DelaySwap();

        // (원한다면 전환 직후 잔여 하이라이트 정리)
        var highlightManager = FindFirstObjectByType<HighlightManager>();
        if (highlightManager) highlightManager.ClearSpellHighlights();

        CurrentTurn = TurnState.PlayerTurn;

        var player = GameObject.FindWithTag("Player");
        if (player == null) { Debug.LogError("Player not found."); yield break; }
        var playerStats = player.GetComponent<Stats>();
        if (playerStats == null) { Debug.LogError("Stats not found on Player."); yield break; }

        // 플레이어 턴 시작 훅
        playerStats.OnTurnStart();

        // 입력/하이라이트 등 플레이어 턴 세팅
        var pc = player.GetComponent<PlayerController>();
        if (pc) pc.StartTurn();

        _finishedPlayerActorCount = 0;
    }

    // (기존 Register/Unregister/Report 관련 함수는 그대로)
    public void RegisterActor() => _totalPlayerActors++;
    public void UnregisterActor() => _totalPlayerActors--;
}
