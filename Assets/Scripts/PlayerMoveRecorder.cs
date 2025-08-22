using System;
using UnityEngine;
using System.Collections.Generic;

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
    private PlayerMoveType previousMove = PlayerMoveType.None;
    public List<int> spellDamages = new List<int>();

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
    
    public void RecordMove(PlayerMoveType moveType, int spellDamage = 0)
    {
        previousMove = moveType;
        if (moveType == PlayerMoveType.Attack && spellDamage > 0)
        {
            spellDamages.Add(spellDamage);
        }
        Debug.Log($"Recorded move: {moveType}");
    }

    public void RecordMove(Vector3Int start, Vector3Int end)
    {
        PlayerMoveType moveType = CalculateMove(start, end);
        RecordMove(moveType);
    }

    public PlayerMoveType GetPreviousMove()
    {
        return previousMove; // PlayerMoveType를 int로 변환하여 반환
    }

    public int GetLastSpellDamage()
    {
        if (spellDamages.Count > 0)
        {
            int lastDamage = spellDamages[spellDamages.Count - 1];
            spellDamages.RemoveAt(spellDamages.Count - 1); // 사용 후 제거
            Debug.Log($"Returning last spell damage: {lastDamage}");
            return lastDamage;
        }
        return 0; // No damage recorded
    }
    
    public int GetSpellDamage()
    {
        if (spellDamages.Count > 0)
        {
            // count 중 랜덤으로 하나를 반환
            int randomIndex = UnityEngine.Random.Range(0, spellDamages.Count);
            int damage = spellDamages[randomIndex];
            spellDamages.RemoveAt(randomIndex); // 사용 후 제거
            Debug.Log($"Returning spell damage: {damage}");
            return damage;
        }
        return 0; // No damage recorded
    }

    public PlayerMoveType CalculateMove(Vector3Int start, Vector3Int end)
    {
        var pc = FindFirstObjectByType<PlayerController>();
        Vector3Int direction = end - start;
        
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
            {
                pc.isTowardsRight = false;
                return PlayerMoveType.Right;
            }
            else
            {
                pc.isTowardsRight = true;
                return PlayerMoveType.Left;
            }
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