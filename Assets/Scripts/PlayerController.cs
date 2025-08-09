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
        if (!isPlayerTurn) return;

        Vector3Int currentCell = tilemap.WorldToCell(transform.position);
        gridManager.ClearOccupantAt(currentCell);
        gridManager.SetOccupantAt(targetCell, gameObject);
        transform.position = tilemap.GetCellCenterWorld(targetCell);

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
        // 턴이 시작됐는데 아직 하이라이트를 안 보여줬다면 표시
        if (isPlayerTurn && !hasShownHighlightThisTurn)
        {
            ShowMoveHighlights();
            hasShownHighlightThisTurn = true;
        }
    }
}