using Kaco.UI.Inventory;
using System.Collections.Generic;
using UnityEngine;

public class LootRegion : MonoBehaviour
{
    [Header("Loot Config")]
    [SerializeField] private LootPoolSO lootPool;

    [Header("Regional Multipliers")]
    [SerializeField] private List<CategoryModifier> categoryModifiers;

    // 런타임에 빠른 검색을 위해 Dictionary로 변환
    private Dictionary<LootCategory, float> _modifierDict;

    private void Awake()
    {
        // 리스트 -> 딕셔너리 캐싱 (최적화)
        _modifierDict = new Dictionary<LootCategory, float>();
        foreach (var mod in categoryModifiers)
        {
            if (!_modifierDict.ContainsKey(mod.Category))
            {
                _modifierDict.Add(mod.Category, mod.Multiplier);
            }
        }
    }

    /// <summary>
    /// 외부(플레이어 상호작용 등)에서 호출하여 아이템 획득
    /// </summary>
    public void GenerateLoot()
    {
        if (lootPool == null) return;

        // 1. 풀에게 "내 구역의 보정치(_modifierDict)"를 주면서 룻을 요청
        List<ItemSO> droppedItems = lootPool.RollLoot(_modifierDict);

        // 2. 결과 처리 (상자에 넣기 or 바닥에 떨구기)
        foreach (var item in droppedItems)
        {
            Debug.Log($"[Loot] {item.Name} 획득! (Category: {item.Category})");
            // Instantiate(item.prefab, transform.position ... )
            // 또는 Chest.AddItem(item) ...
        }
    }
}
