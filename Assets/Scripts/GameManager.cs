using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public TileManager tileManager;
    public Tilemap tilemap;        // 시작 위치 기준
    public GameObject player;

    void Start()
    {
        Vector3Int startCell = tileManager.FindStartTilePosition();
        Vector3 worldPos = tilemap.GetCellCenterWorld(startCell);
        player.transform.position = worldPos;
        player.SetActive(true);
    }

    void Update()
    {
        // 게임 로직 업데이트 (예: 턴 관리, 승리 조건 체크 등)
    }

    public void NextTurn()
    {
        // 턴 전환 로직
        // 예: 적의 턴으로 전환, 승리 조건 체크 등
        Debug.Log("턴이 전환되었습니다.");
    }
}
