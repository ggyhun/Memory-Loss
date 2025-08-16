using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class HighlightManager : MonoBehaviour
{
    public GameObject player;
    public GridManager gridManager; // GridManager 스크립트 참조
    
    public List<GameObject> moveHighlighters; // 이동 가능한 타일을 표시할 하이라이터 오브젝트들
    public GameObject moveHighlighterPrefab; // 이동 하이라이터 프리팹

    public List<GameObject> castHighlighters; // 시전 가능한 타일을 표시할 하이라이터 오브젝트들
    public GameObject castHighlighterPrefab;

    private readonly Vector3Int[] _castHighlightDirections =
    {
        Vector3Int.zero,
        Vector3Int.up + Vector3Int.left,
        Vector3Int.up,
        Vector3Int.up + Vector3Int.right,
        Vector3Int.left,
        Vector3Int.zero,
        Vector3Int.right,
        Vector3Int.down + Vector3Int.left,
        Vector3Int.down,
        Vector3Int.down + Vector3Int.right
    };
    
    private void Awake()
    {
        // 기본적으로 4개 생성후 리스트에 추가
        for (int i = 0; i < 4; i++)
        {
            GameObject highlighter = Instantiate(moveHighlighterPrefab);
            highlighter.SetActive(false); // 초기에는 비활성화
            moveHighlighters.Add(highlighter);
        }

        for (int i = 1; i <= 9; i++)
        {
            if (i == 5) continue; // 중앙은 제외
            GameObject highlighter = Instantiate(castHighlighterPrefab);
            highlighter.transform.position = player.transform.position + _castHighlightDirections[i];
            highlighter.SetActive(false); // 초기에는 비활성화
            castHighlighters.Add(highlighter);
        }
    }

    private void Start()
    {
        TurnManager.Instance.RegisterActor();
        
        // 플레이어의 위치에 따라 하이라이터 업데이트
        UpdateMoveHighlighter();
    }
    
    public void HandleMoveHighlighterClick(Vector3 position)
    {
        gridManager.MoveTo(player, position);
        ClearMoveHighlighters();
        
        TurnManager.Instance.PlayerReportDone();
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
                continue;
            }
            
            // 하이라이터 위치 업데이트
            moveHighlighters[i].SetActive(true);
            moveHighlighters[i].transform.position = targetPos;
            i++;
        }
    }
    
    private void ClearMoveHighlighters()
    {
        // 모든 하이라이터 비활성화
        foreach (GameObject highlighter in moveHighlighters)
        {
            highlighter.SetActive(false);
        }
    }
}
