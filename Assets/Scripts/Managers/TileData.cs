using UnityEngine;

public class TileData
{
    // 논리 상태
    public bool isWalkable = true;
    public bool isTrap = false;
    public GameObject occupant = null;
    public bool isExit = false;

    // 시각 레이어 정보 (optional)
    // public TileBase backgroundTile;
    // public TileBase obstacleTile;
    // public TileBase decorativeTile;
}
