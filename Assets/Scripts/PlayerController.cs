using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    // 플레이어 컨트롤러 스크립트, 플레이어의 컴포넌트
    [Header("References")]
    public Tilemap backgroundTilemap; // 이동 기준이 되는 Tilemap (Background)
    public GridManager gridManager; // 타일 논리 정보 관리
    public HighlightManager highlightManager; // 하이라이트 표시 및 클릭 관리

    [Header("Turn Control")]
    public bool isPlayerTurn = true;

    private void Awake()
    {
        // 초기화 작업
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (highlightManager == null) highlightManager = FindFirstObjectByType<HighlightManager>();
        
        // 플레이어의 시작 위치를 GridManager에서 가져옴
        Vector3Int startTilePosition = gridManager.FindStartTilePosition();
        transform.position = backgroundTilemap.GetCellCenterWorld(startTilePosition);
    }
}