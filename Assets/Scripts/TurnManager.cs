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

    // ===== 애니 끝 신호 대기 플래그 =====
    // 플레이어 행동(공격/이동 연출 등) 후, 애니메이션 끝에서 턴을 넘기고 싶을 때 true
    private bool waitPlayerAnimToEndTurn = false;

    // 전환 코루틴 중복 방지
    private bool isSwapping = false;

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

    // ====== 애니메이션 대기 API ======
    /// <summary>
    /// 플레이어 액션 직전에 호출: 애니메이션이 끝나야 턴을 넘기도록 예약
    /// (예: PlayerAnimator.PlayAttackAnimation() 내부)
    /// </summary>
    public void RequestEndAfterPlayerAnimation()
    {
        waitPlayerAnimToEndTurn = true;
    }

    /// <summary>
    /// 애니메이션 이벤트(클립 마지막 프레임 등)에서 호출:
    /// 예약이 있었다면 여기서 실제로 턴 종료 진행
    /// </summary>
    public void NotifyPlayerAnimationComplete()
    {
        if (!waitPlayerAnimToEndTurn) return;
        waitPlayerAnimToEndTurn = false;
        EndPlayerTurn(); // 이제 보류 없이 진행
    }

    // ===== 플레이어 → 시스템 =====
    public void EndPlayerTurn()
    {
        // 애니 끝을 기다리기로 한 상태라면, 여기서는 보류
        if (waitPlayerAnimToEndTurn)
            return;

        if (!isSwapping)
            StartCoroutine(CoEndPlayerTurn());
    }

    private IEnumerator CoEndPlayerTurn()
    {
        isSwapping = true;

        var player = GameObject.FindWithTag("Player");
        if (player == null) { Debug.LogError("Player not found."); isSwapping = false; yield break; }
        var playerStats = player.GetComponent<Stats>();
        if (playerStats == null) { Debug.LogError("Stats not found on Player."); isSwapping = false; yield break; }

        // 플레이어 턴 종료 훅
        playerStats.OnTurnEnd();

        _finishedPlayerActorCount = 0;
        CurrentTurn = TurnState.SystemTurn;

        // 전환 딜레이
        yield return DelaySwap();

        // 적 턴 시작
        enemyManager.StartEnemyTurn();

        isSwapping = false;
    }

    // ===== 시스템 → 플레이어 =====
    public void StartPlayerTurn()
    {
        if (!isSwapping)
            StartCoroutine(CoStartPlayerTurn());
    }

    private IEnumerator CoStartPlayerTurn()
    {
        isSwapping = true;

        // 전환 딜레이
        yield return DelaySwap();

        // (원한다면 전환 직후 잔여 하이라이트 정리)
        var highlightManager = FindFirstObjectByType<HighlightManager>();
        if (highlightManager) highlightManager.ClearSpellHighlights();

        CurrentTurn = TurnState.PlayerTurn;

        var player = GameObject.FindWithTag("Player");
        if (player == null) { Debug.LogError("Player not found."); isSwapping = false; yield break; }
        var playerStats = player.GetComponent<Stats>();
        if (playerStats == null) { Debug.LogError("Stats not found on Player."); isSwapping = false; yield break; }

        // 플레이어 턴 시작 훅
        playerStats.OnTurnStart();

        // 입력/하이라이트 등 플레이어 턴 세팅
        var pc = player.GetComponent<PlayerController>();
        if (pc) pc.StartTurn();

        _finishedPlayerActorCount = 0;
        isSwapping = false;
    }

    // (기존 Register/Unregister/Report 관련 함수는 유지)
    public void RegisterActor()   => _totalPlayerActors++;
    public void UnregisterActor() => _totalPlayerActors--;
}
