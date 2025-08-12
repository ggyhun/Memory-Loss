using UnityEngine;

public class TileData
{
    public Vector3Int cellPosition; // 타일의 셀 상대 위치
    public Vector3 worldPosition; // 타일의 월드 위치 : Vector3로 사용
    
    // 논리 상태
    public bool isWalkable = true;
    public GameObject occupant = null;
    public bool isExit = false;
}
