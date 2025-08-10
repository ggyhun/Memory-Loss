using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Tilemap tilemap; // 이동 기준이 되는 Tilemap (Background)
    public GridManager gridManager; // 타일 논리 정보 관리
    public HighlightManager highlightManager; // 하이라이트 표시 및 클릭 관리

    [Header("Turn Control")]
    public bool isPlayerTurn = true;
    
    private Vector2Int PlayerPos => Vector2Int.RoundToInt(transform.position);
    [SerializeField] private SpellInventory spellInventory;

    // 플레이어의 현재 클릭 상태 (이동 또는 공격), 주문서 클릭시 공격으로 변경
    private enum ClickState
    {
        Move,
        Attack
    };
    private ClickState currentClickState = ClickState.Move;
    

    void Start()
    {
        ShowMoveHighlights();
    }

    void ShowMoveHighlights()
    {
        if (!isPlayerTurn) return;

        Vector3Int playerCell = tilemap.WorldToCell(transform.position);
        highlightManager.ShowHighlights(playerCell, OnHighlightClick);
    }

    void OnHighlightClick(Vector3Int targetCell)
    {
        // 플레이어 턴이 아닐 때는 아무 동작도 하지 않음
        if (!isPlayerTurn) return;
        
        if (currentClickState == ClickState.Move)
        {
            // Player 위치 이동
            Vector3Int currentCell = tilemap.WorldToCell(transform.position);
            gridManager.ClearOccupantAt(currentCell);
            gridManager.SetOccupantAt(targetCell, gameObject);
            transform.position = tilemap.GetCellCenterWorld(targetCell);
        }
        
        if (currentClickState == ClickState.Attack)
        {
            // Todo: 공격 로직 추가 (예: 적과 충돌 처리)
            Debug.Log("공격 로직은 아직 구현되지 않았습니다.");
        }
        
        // 턴 종료
        isPlayerTurn = false;
        hasShownHighlightThisTurn = false;

        highlightManager.ClearHighlights();

        // 이후: Enemy 턴 → 다시 isPlayerTurn = true 설정 시 Highlight 표시됨
        
        // Todo: 적의 턴 로직 추가
        isPlayerTurn = true;
    }


    private bool hasShownHighlightThisTurn = false;

    void Update()
    {
        // 1번 키를 눌렀을 때 첫 번째 스펠 사용
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            spellInventory.UseSpell(0, PlayerPos);
        }
        
        // 턴이 시작됐는데 아직 하이라이트를 안 보여줬다면 표시
        if (isPlayerTurn && !hasShownHighlightThisTurn)
        {
            ShowMoveHighlights();
            hasShownHighlightThisTurn = true;
        }
    }
    
    private void HighlightClickConverter(Vector3Int targetCell)
    {
        if (!isPlayerTurn) return;

        Vector3Int currentCell = tilemap.WorldToCell(transform.position);
        TileData data = gridManager.GetTileData(targetCell);

        if (data != null && data.isWalkable && data.occupant == null)
        {
            gridManager.ClearOccupantAt(currentCell);
            gridManager.SetOccupantAt(targetCell, gameObject);
            transform.position = tilemap.GetCellCenterWorld(targetCell);
            isPlayerTurn = false;
            hasShownHighlightThisTurn = false;
            highlightManager.ClearHighlights();
        }
    }

    
    // 주문서 습득 시 호출
    public void PickUpSpell(SpellData spell)
    {
        spellInventory.AddSpell(spell);
    }
}