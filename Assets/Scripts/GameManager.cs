using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public GridManager gridManager;
    public Tilemap tilemap; // 시작 위치 기준
    public GameObject player;

    // Todo: 이 스크립트는 도대체 무엇을 할까요? 제가 만들었는데 저도 모르겠어요 - 경훈
    
    void Start()
    {
        Vector3Int startCell = gridManager.FindStartTilePosition();
        Vector3 worldPos = tilemap.GetCellCenterWorld(startCell);
        player.transform.position = worldPos;
        player.SetActive(true);
    }
}
