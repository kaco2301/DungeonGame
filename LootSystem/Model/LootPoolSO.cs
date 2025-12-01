using Kaco.UI.Inventory;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Loot/Loot Pool")]
public class LootPoolSO : ScriptableObject
{
    [Header("Settings")]
    public int MinDropCount = 1;
    public int MaxDropCount = 3;

    [Header("Tables")]
    [Tooltip("이 풀에 포함된 테이블들 (예: Common, Rare, RegionSpecific)")]
    public List<LootTableSO> LootTables;

    /// <summary>
    /// 이 풀의 규칙에 따라 아이템 리스트를 생성하여 반환합니다.
    /// </summary>
    public List<ItemSO> RollLoot(Dictionary<LootCategory, float> modifiers)
    {
        List<ItemSO> results = new List<ItemSO>();
        int dropCount = Random.Range(MinDropCount, MaxDropCount + 1);

        for (int i = 0; i < dropCount; i++)
        {
            // 1. 어느 테이블에서 뽑을지 먼저 결정 (균등 확률 혹은 테이블별 가중치 추가 가능)
            // 여기서는 단순하게 랜덤 테이블 선택 예시
            if (LootTables.Count > 0)
            {
                var selectedTable = LootTables[Random.Range(0, LootTables.Count)];
                var item = selectedTable.GetItem(modifiers);

                if (item != null)
                    results.Add(item);
            }
        }

        return results;
    }
}
