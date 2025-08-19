using System;
using UnityEngine;

public enum PlayerMoveType
{
    None, // 초기값, 이동하지 않음
    Up,
    Down,
    Left,
    Right,
    Attack
}

public class PlayerMoveRecorder : MonoBehaviour
{
    public static PlayerMoveRecorder Instance { get; private set; }
    public PlayerMoveType previousMove = PlayerMoveType.None;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void RecordMove(PlayerMoveType moveType)
    {
        previousMove = moveType;
        Debug.Log($"Recorded move: {moveType}");
    }

    public PlayerMoveType CalculateMove(Vector3Int start, Vector3Int end)
    {
        Vector3Int direction = end - start;
        
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
                return PlayerMoveType.Right;
            else
                return PlayerMoveType.Left;
        }
        else if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
        {
            if (direction.y > 0)
                return PlayerMoveType.Up;
            else
                return PlayerMoveType.Down;
        }
        
        return PlayerMoveType.None;
    }
}