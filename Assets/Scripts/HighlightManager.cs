using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class HighlightManager : MonoBehaviour
{
    public GameObject player;
    public GridManager gridManager; // GridManager 스크립트 참조
    
    public List<GameObject> MoveHighlighters; // 이동 가능한 타일을 표시할 하이라이터 오브젝트들
    public GameObject moveHighlighterPrefab; // 이동 하이라이터 프리팹
    
    private void Awake()
    {
        // 기본적으로 4개 생성후 리스트에 추가
        for (int i = 0; i < 4; i++)
        {
            GameObject highlighter = Instantiate(moveHighlighterPrefab);
            highlighter.SetActive(false); // 초기에는 비활성화
            MoveHighlighters.Add(highlighter);
        }
    }

    private void Start()
    {
        // 플레이어의 위치에 따라 하이라이터 업데이트
        UpdateMoveHighlighter();
    }
    
    public void HandleMoveHighlighterClick(Vector3 position)
    {
        // 클릭된 위치의 타일 데이터 가져오기
        TileData targetTile = gridManager.GetTileData(position);

        // 이동 가능 여부 확인
        if (targetTile.isWalkable && targetTile.occupant == null) {
            TileData playerTile = gridManager.GetTileData(player.transform.position);
            
            // 점유자 변경
            targetTile.occupant = playerTile.occupant;
            playerTile.occupant = null;
            
            // 플레이어를 새로운 위치로 이동
            player.transform.position = targetTile.worldPosition;
        }
        else
        {
            Debug.Log("이동 불가능 / 하이라이트 점검 필요 : " + position);
        }
        
        UpdateMoveHighlighter();
    }
    
    public void UpdateMoveHighlighter()
    {
        // 플레이어의 현재 위치를 기준으로 하이라이터 위치 업데이트
        TileData playerTile = gridManager.GetTileData(player.transform.position);
        // 예외 처리
        if (playerTile == null || !playerTile.isWalkable)
        {
            Debug.LogWarning("플레이어가 있는 타일이 유효하지 않거나 이동 불가능합니다.");
            return;
        }
        
        Vector3 playerPos;
        Vector3Int[] directions = {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        ClearMoveHighlighters();

        int i = 0;
        foreach (Vector3Int direction in directions) {
            playerPos = player.transform.position;
            Vector3 targetPos = playerPos + direction;
            Vector3Int targetCellPos = gridManager.backgroundMap.WorldToCell(targetPos);

            // 타일 데이터 가져오기
            TileData targetTile = gridManager.GetTileData(targetCellPos);

            // 예외 처리
            if (targetTile == null || !targetTile.isWalkable || targetTile.occupant) {
                Debug.LogWarning("이동 불가능한 타일: " + targetPos);
                continue;
            }
            
            // 하이라이터 위치 업데이트
            MoveHighlighters[i].SetActive(true);
            MoveHighlighters[i].transform.position = targetPos;
            i++;
        }
    }
    
    private void ClearMoveHighlighters()
    {
        // 모든 하이라이터 비활성화
        foreach (GameObject highlighter in MoveHighlighters)
        {
            highlighter.SetActive(false);
        }
    }
}
