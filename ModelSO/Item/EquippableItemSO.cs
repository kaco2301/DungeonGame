using System.Collections.Generic;
using UnityEngine;

namespace Kaco.UI.Inventory
{
    [CreateAssetMenu(menuName = "ItemSO/EquippableItemSO", fileName = "EquippableItem")]
    public class EquippableItemSO : ItemSO, IHasDisplayableModifiers
    {
        [field: SerializeField]
        public Define.EquipmentType EquipmentType { get; private set; }
        
        [field: SerializeField]
        public List<ModifierData> statModifiers { get; private set; } = new List<ModifierData>();

        protected void SetEquipmentType(Define.EquipmentType type)
        {
            EquipmentType = type;
        }

        public List<DisplayableModifier> GetModifiersForDisplay()
        {
            var displayList = new List<DisplayableModifier>();
            if (statModifiers == null) return displayList;

            foreach (var modData in statModifiers)
            {
                if (modData.statModifier == null) continue;
                displayList.Add(new DisplayableModifier
                {
                    Icon = modData.statModifier.Icon,
                    DisplayText = $"+{modData.value} {modData.statModifier.StatName}"
                });
            }
            return displayList;
        }

        public override Define.ItemRarity Rarity
        {
            get
            {
                int modifierCount = statModifiers?.Count ?? 0;

                switch (modifierCount)
                {
                    case 0: return Define.ItemRarity.Common;
                    case 1: return Define.ItemRarity.Rare;
                    case 2: return Define.ItemRarity.Unique;
                    case 3: return Define.ItemRarity.Epic;
                    case 4:
                    default: // 4�� �̻�
                        return Define.ItemRarity.Legendary;
                }
            }
        }
    }
}

