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
}