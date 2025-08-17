using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Level Data", fileName = "LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Level")]
    public int floorIndex = 1;

    [Header("Enemy Spawn")]
    [Min(0)] public int enemyCount = 3;
    public List<EnemySpawnEntry> enemyTable = new List<EnemySpawnEntry>();

    [Header("Scroll Spawn")]
    [Min(0)] public int scrollCount = 3;
    public List<ScrollSpawnEntry> scrollTable = new List<ScrollSpawnEntry>();

    [Serializable]
    public class EnemySpawnEntry
    {
        public string displayName;              // 예: "슬라임 (Slim)"
        public GameObject enemyPrefab;          // 해당 적 프리팹
        [Range(0, 100)] public int weight = 1;  // 확률 가중치 (합 100일 필요는 없음)
    }

    [Serializable]
    public class ScrollSpawnEntry
    {
        public string displayName;              // 예: "얼음의 주문서"
        public SpellData spell;                 // 스크롤에 담길 SpellData (공격/소비형 모두 포함)
        [Range(0, 100)] public int weight = 1;  // 확률 가중치
    }

    /// <summary>가중치 랜덤으로 적 프리팹 하나 선택</summary>
    public GameObject PickEnemyPrefab(System.Random rng)
    {
        if (enemyTable == null || enemyTable.Count == 0) return null;
        int total = 0;
        foreach (var e in enemyTable) total += Mathf.Max(0, e.weight);
        if (total <= 0) return null;

        int roll = rng.Next(0, total); // [0, total)
        int acc = 0;
        foreach (var e in enemyTable)
        {
            acc += Mathf.Max(0, e.weight);
            if (roll < acc) return e.enemyPrefab;
        }
        return enemyTable[enemyTable.Count - 1].enemyPrefab;
    }

    /// <summary>가중치 랜덤으로 SpellData 하나 선택</summary>
    public SpellData PickScrollSpell(System.Random rng)
    {
        if (scrollTable == null || scrollTable.Count == 0) return null;
        int total = 0;
        foreach (var s in scrollTable) total += Mathf.Max(0, s.weight);
        if (total <= 0) return null;

        int roll = rng.Next(0, total);
        int acc = 0;
        foreach (var s in scrollTable)
        {
            acc += Mathf.Max(0, s.weight);
            if (roll < acc) return s.spell;
        }
        return scrollTable[scrollTable.Count - 1].spell;
    }
}
