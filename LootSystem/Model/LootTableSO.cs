using Kaco.UI.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum LootCategory
{
    None,
    Weapon,
    Armor,
    Potion,
    Material,
    Quest

}
[System.Serializable]
public class LootEntry
{
    public ItemSO Item;
    public float BaseWeight = 10f;
    public int minAmount = 1;
    public int maxAmount = 1;

}

[Serializable]
public struct CategoryModifier
{
    public LootCategory Category;
    [Tooltip("가중치 배수 (예: 2.0이면 확률 2배)")]
    public float Multiplier;
}

[CreateAssetMenu(menuName = "Game/Loot/Loot Table")]
public class LootTableSO : ScriptableObject
{
    public List<LootEntry> Entries = new List<LootEntry>();

    /// <summary>
    /// 지역 보정치를 받아 최종 가중치가 적용된 아이템 하나를 뽑습니다.
    /// </summary>
    public ItemSO GetItem(Dictionary<LootCategory, float> modifiers)
    {
        float totalWeight = 0f;

        // 1. 전체 가중치 합 계산 (보정치 적용)
        foreach (var entry in Entries)
        {
            float multiplier = 1f;
            // 아이템에 카테고리가 있다고 가정 (없다면 ItemSO에 추가 필요)
            // modifiers에 해당 카테고리가 있으면 배율 적용
            if (modifiers != null && modifiers.TryGetValue(entry.Item.Category, out float mod))
            {
                multiplier = mod;
            }

            totalWeight += entry.BaseWeight * multiplier;
        }

        // 2. 랜덤 뽑기
        float randomValue = UnityEngine.Random.Range(0, totalWeight);
        float currentWeight = 0f;

        foreach (var entry in Entries)
        {
            float multiplier = 1f;
            if (modifiers != null && modifiers.TryGetValue(entry.Item.Category, out float mod))
            {
                multiplier = mod;
            }

            float effectiveWeight = entry.BaseWeight * multiplier;
            currentWeight += effectiveWeight;

            if (randomValue <= currentWeight)
            {
                return entry.Item;
            }
        }

        return null; // 예외 상황 (보통 여기 오기 전 리턴됨)
    }
}
