using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public enum TurnState { PlayerTurn, SystemTurn }
    public TurnState CurrentTurn { get; private set; }

    [Header("Swap Delay")]
    public float turnSwapDelay = 1f;
    public bool useUnscaledTime = true;

    // === Extra Turn ===
    // 외부(주문서 등)에서 SetExtraTurn()으로 누적 예약
    [SerializeField] private int extraTurnsPending = 0;
    public bool isExtraTurn => extraTurnsPending > 0;  // 읽기 전용 뷰

    private int _totalPlayerActors;
    private int _finishedPlayerActorCount;

    [SerializeField] private EnemyManager enemyManager;

    // 애니 끝까지 기다리기
    private bool waitPlayerAnimToEndTurn = false;

    // 전환 중복 방지
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

    // ===== Extra Turn API =====
    /// <summary>
    /// 추가 턴을 n회 예약. (기본 1회)
    /// </summary>
    public void SetExtraTurn(int count = 1)
    {
        if (count <= 0) return;
        extraTurnsPending += count;
        // 필요시 디버그:
        // Debug.Log($"[TurnManager] Extra turns +{count} → pending: {extraTurnsPending}");
    }

    private void ConsumeOneExtraTurn()
    {
        if (extraTurnsPending > 0) extraTurnsPending--;
        // 필요시 디버그:
        // Debug.Log($"[TurnManager] Extra turn consumed. pending: {extraTurnsPending}");
    }

    // ===== 전환 코루틴 헬퍼 =====
    private IEnumerator DelaySwap()
    {
        if (turnSwapDelay <= 0f) yield break;
        if (useUnscaledTime) yield return new WaitForSecondsRealtime(turnSwapDelay);
        else                 yield return new WaitForSeconds(turnSwapDelay);
    }

    // ===== 애니 대기 플래그 =====
    public void RequestEndAfterPlayerAnimation() => waitPlayerAnimToEndTurn = true;

    public void NotifyPlayerAnimationComplete()
    {
        if (!waitPlayerAnimToEndTurn) return;
        waitPlayerAnimToEndTurn = false;
        EndPlayerTurn();
    }

    // ===== 플레이어 → 시스템 (혹은 추가 턴으로 바로 회귀) =====
    public void EndPlayerTurn()
    {
        if (waitPlayerAnimToEndTurn) return;      // 애니 끝날 때까지 보류
        if (!isSwapping) StartCoroutine(CoEndPlayerTurn());
    }

    private IEnumerator CoEndPlayerTurn()
    {
        isSwapping = true;

        var player = GameObject.FindWithTag("Player");
        if (player == null) { Debug.LogError("Player not found."); isSwapping = false; yield break; }
        var playerStats = player.GetComponent<Stats>();
        if (playerStats == null) { Debug.LogError("Stats not found on Player."); isSwapping = false; yield break; }

        // 플레이어 턴 종료 훅 (도트/지속시간 감소 등)
        playerStats.OnTurnEnd();

        _finishedPlayerActorCount = 0;

        // ⛳️ 추가 턴이 예약되어 있으면: 적 턴을 스킵하고 곧장 플레이어 추가 턴으로
        if (isExtraTurn)
        {
            ConsumeOneExtraTurn();

            // (원하면 살짝 연출 딜레이)
            yield return DelaySwap();

            // 적 턴으로 가지 않고, 플레이어 턴을 다시 시작
            CurrentTurn = TurnState.PlayerTurn;

            // 🔸 간단히 StartTurn 재사용 (쿨다운/OnTurnStart가 다시 돌 수 있음)
            //    만약 ‘추가턴에서는 쿨다운/도트 반복 NO’가 필요하면,
            //    PlayerController에 StartExtraTurn()을 만들어 isSpellSelected만 초기화하고
            //    HighlightManager.ShowMoveHighlighters()만 호출하세요.
            var pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.StartTurn();
            else Debug.LogWarning("PlayerController not found on Player.");

            isSwapping = false;
            yield break;
        }

        // 추가 턴이 아니면 정상적으로 적 턴으로 전환
        CurrentTurn = TurnState.SystemTurn;

        yield return DelaySwap();

        enemyManager.StartEnemyTurn();

        isSwapping = false;
    }

    // ===== 시스템 → 플레이어 =====
    public void StartPlayerTurn()
    {
        if (!isSwapping) StartCoroutine(CoStartPlayerTurn());
    }

    private IEnumerator CoStartPlayerTurn()
    {
        isSwapping = true;

        yield return DelaySwap();

        var highlightManager = FindFirstObjectByType<HighlightManager>();
        if (highlightManager) highlightManager.ClearSpellHighlights();

        CurrentTurn = TurnState.PlayerTurn;

        var player = GameObject.FindWithTag("Player");
        if (player == null) { Debug.LogError("Player not found."); isSwapping = false; yield break; }
        var playerStats = player.GetComponent<Stats>();
        if (playerStats == null) { Debug.LogError("Stats not found on Player."); isSwapping = false; yield break; }

        // 플레이어 턴 시작 훅
        playerStats.OnTurnStart();

        // 입력/하이라이트 등 세팅
        var pc = player.GetComponent<PlayerController>();
        if (pc) pc.StartTurn();

        _finishedPlayerActorCount = 0;
        isSwapping = false;
    }

    // (기존 Register/Unregister/Report 관련 함수는 유지)
    public void RegisterActor()   => _totalPlayerActors++;
    public void UnregisterActor() => _totalPlayerActors--;
}
