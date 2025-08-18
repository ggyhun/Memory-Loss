using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Level Data", fileName = "LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Level")]
    public int floorIndex = 1;

    // ---------------- Enemy ----------------
    [Header("Enemy Spawn")]
    [Min(0)] public int enemyCount = 3;
    public List<EnemySpawnEntry> enemyTable = new List<EnemySpawnEntry>();

    [Serializable]
    public class EnemySpawnEntry
    {
        public string displayName;
        public GameObject enemyPrefab;
        [Range(0, 100)] public int weight = 1;
    }

    // ---------------- Scrolls ----------------
    [Header("Scroll Spawn")]
    [Min(0)] public int scrollCount = 3;

    [Tooltip("이 층에서 사용될 스크롤 프리팹 + 가중치 풀 (예: Scroll_Fire, Scroll_Ice, Scroll_Wet 등)")]
    public List<ScrollPrefabEntry> scrollPrefabTable = new List<ScrollPrefabEntry>();

    [Tooltip("프리팹의 Scroll.spellData 가 비었을 때 사용할 스펠 풀(가중치)")]
    public List<ScrollSpellEntry> scrollSpellTable = new List<ScrollSpellEntry>();

    [Serializable]
    public class ScrollPrefabEntry
    {
        public string displayName;
        public GameObject prefab;               // 예: Scroll_Fire 프리팹
        [Range(0, 100)] public int weight = 1;
    }

    [Serializable]
    public class ScrollSpellEntry
    {
        public string displayName;
        public SpellData spell;                 // 예: FireBall, IceMissile 등
        [Range(0, 100)] public int weight = 1;
    }

    // --------- Pickers ---------
    public GameObject PickEnemyPrefab(System.Random rng)
    {
        if (enemyTable == null || enemyTable.Count == 0) return null;
        int total = 0;
        foreach (var e in enemyTable) total += Mathf.Max(0, e.weight);
        if (total <= 0) return null;

        int roll = rng.Next(0, total), acc = 0;
        foreach (var e in enemyTable)
        {
            acc += Mathf.Max(0, e.weight);
            if (roll < acc) return e.enemyPrefab;
        }
        return enemyTable[enemyTable.Count - 1].enemyPrefab;
    }

    public GameObject PickScrollPrefab(System.Random rng)
    {
        if (scrollPrefabTable == null || scrollPrefabTable.Count == 0) return null;
        int total = 0;
        foreach (var e in scrollPrefabTable) total += Mathf.Max(0, e.weight);
        if (total <= 0) return null;

        int roll = rng.Next(0, total), acc = 0;
        foreach (var e in scrollPrefabTable)
        {
            acc += Mathf.Max(0, e.weight);
            if (roll < acc) return e.prefab;
        }
        return scrollPrefabTable[scrollPrefabTable.Count - 1].prefab;
    }

    public SpellData PickScrollSpell(System.Random rng)
    {
        if (scrollSpellTable == null || scrollSpellTable.Count == 0) return null;
        int total = 0;
        foreach (var s in scrollSpellTable) total += Mathf.Max(0, s.weight);
        if (total <= 0) return null;

        int roll = rng.Next(0, total), acc = 0;
        foreach (var s in scrollSpellTable)
        {
            acc += Mathf.Max(0, s.weight);
            if (roll < acc) return s.spell;
        }
        return scrollSpellTable[scrollSpellTable.Count - 1].spell;
    }
}
