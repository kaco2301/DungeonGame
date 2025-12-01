using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kaco.UI.Inventory
{
    public abstract class ItemSO : ScriptableObject
    {
        [field : SerializeField]
        private string itemID;
        public string ID => itemID;

        public LootCategory Category;

        [field: SerializeField]
        public bool IsStackable { get; set; }

        [field: SerializeField]
        public int MaxStackSize { get; set; } = 1;

        [field: SerializeField]
        public string Name { get; set; }

        [field: SerializeField]
        [field: TextArea]
        public string Description { get; set; }

        [field: SerializeField]
        public Sprite Icon { get; set; }

        public virtual bool IsDestroyable => false;

        public virtual Define.ItemRarity Rarity => Define.ItemRarity.Common;

        [field: SerializeField]
        public List<ItemParameter> DefaultParametersList { get; set; }

    }

    [Serializable]
    public struct ItemParameter : IEquatable<ItemParameter>
    {
        public ItemParameterSO itemParameter;
        public float value;

        public bool Equals(ItemParameter other)
        {
            return other.itemParameter == itemParameter;
        }
    }

    public struct DisplayableModifier
    {
        public Sprite Icon;
        public string DisplayText; // (예: "+5 Strength", "+15 HP")
    }

    /// <summary>
    /// 툴팁에 모디파이어/효과 리스트를 표시할 수 있는 모든 ItemSO가
    /// 구현해야 하는 인터페이스
    /// </summary>
    public interface IHasDisplayableModifiers
    {
        /// <summary>
        /// 툴팁에 표시할 모디파이어 목록을 반환합니다.
        /// </summary>
        List<DisplayableModifier> GetModifiersForDisplay();
    }

    public interface IItemAction
    {
        public string ActionName { get; }
        public AudioClip actionSFX { get; }
        bool PerformAction(GameObject character, List<ItemParameter> itemState);
    }
}

