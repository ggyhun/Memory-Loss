using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public enum TurnState { PlayerTurn, SystemTurn }
    public TurnState CurrentTurn { get; private set; }

    [SerializeField] private EnemyManager enemyManager;

    private void Awake()
    {
        Instance = this;
        CurrentTurn = TurnState.PlayerTurn;
    }

    // PlayerController에서 호출
    public void EndPlayerTurn()
    {
        CurrentTurn = TurnState.SystemTurn;
        enemyManager.StartEnemyTurn();
    }

    public void StartPlayerTurn()
    {
        CurrentTurn = TurnState.PlayerTurn;
        // Todo: 플레이어 턴 시작 로직 추가
        // UI 활성화, 플레이어 입력 허용
    }
}