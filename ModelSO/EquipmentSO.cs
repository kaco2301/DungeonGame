using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Kaco.UI.Inventory
{
    public interface IEquipmentStorage : IItemStorage
    {
        InventoryItem GetItemAt(Define.EquipmentType type);
        InventoryItem Equip(Define.EquipmentType type, InventoryItem item);
        InventoryItem UnEquip(Define.EquipmentType type);
    }

    /// <summary>
    /// 장비창 데이터를 관리하는 ScriptableObject.
    /// 각 EquipmentType별로 착용된 아이템을 저장하며,
    /// 장비 장착/해제/조회/검증 등의 기능을 제공한다.
    /// </summary>
    [CreateAssetMenu(fileName = "Equipment", menuName = "Inventory/Equipment")]
    public class EquipmentSO : ScriptableObject, IEquipmentStorage
    {
        private static readonly Define.EquipmentType[] SlotOrder =
        {
            Define.EquipmentType.Helmet,
            Define.EquipmentType.Armor,
            Define.EquipmentType.Gloves,
            Define.EquipmentType.Boots,
            Define.EquipmentType.Weapon,
            Define.EquipmentType.Amulet,
            Define.EquipmentType.Ring01,
            Define.EquipmentType.Ring02,
            Define.EquipmentType.SubWeapon,
        };

        /// <summary>
        /// 실제 장비 데이터 저장 딕셔너리
        /// </summary>
        private Dictionary<Define.EquipmentType, InventoryItem> _equipmentSlots;

        /// <summary>
        /// 장비 슬롯 변경 시 발생하는 이벤트.
        /// (변경된 슬롯 타입과 새로운 아이템이 전달됨)
        /// </summary>
        public event Action<Define.EquipmentType, InventoryItem> OnEquipmentSlotChanged;

        /// <summary>
        /// 현재 장비 슬롯의 총 개수
        /// </summary>
        public int Capacity => SlotOrder.Length;


        public void Initialize()
        {
            if (_equipmentSlots != null) return;

            _equipmentSlots = new Dictionary<Define.EquipmentType, InventoryItem>(SlotOrder.Length);
            foreach (var t in SlotOrder)
                _equipmentSlots[t] = InventoryItem.GetEmptyItem();
        }

        /// <summary>
        /// 유효한 슬롯 인덱스인지 검사한다.
        /// </summary>
        public bool IsValidIndex(int index) => index >= 0 && index < Capacity;

        /// <summary>
        /// 특정 장비 타입의 현재 아이템을 가져온다.
        /// </summary>
        public InventoryItem GetItemAt(Define.EquipmentType type)
        {
            if (_equipmentSlots.TryGetValue(type, out var item))
                return item;
            return InventoryItem.GetEmptyItem();
        }

        /// <summary>
        /// 인덱스 기반으로 현재 아이템을 가져온다.
        /// 내부적으로 SlotOrder[index]를 타입으로 변환하여 조회한다.
        /// </summary>
        public InventoryItem GetItemAt(int index)
        {
            if (!IsValidIndex(index)) return InventoryItem.GetEmptyItem();
            return GetItemAt(SlotOrder[index]);
        }

        /// <summary>
        /// 특정 타입이 SlotOrder 내에서 몇 번째 인덱스인지 반환한다.
        /// (UI 슬롯과의 매핑에 사용)
        /// </summary>
        public int GetIndexOfType(Define.EquipmentType type)
        {
            for (int i = 0; i < SlotOrder.Length; i++)
                if (SlotOrder[i] == type) return i;
            return -1;
        }

        /// <summary>
        /// 현재 장비 상태 전체를 읽기 전용으로 반환한다.
        /// 외부에서 직접 수정할 수 없도록 ReadOnlyDictionary로 감싼다.
        /// </summary>
        public IReadOnlyDictionary<Define.EquipmentType, InventoryItem> GetCurrentEquipmentState()
        {
            return new ReadOnlyDictionary<Define.EquipmentType, InventoryItem>(_equipmentSlots);
        }


        #region 장비 장착 및 해제 메서드 =================================================================================

        /// <summary>
        /// 아이템을 장착 시도한다.
        /// - 타입이 맞지 않거나 장착 불가능한 경우 false 반환.
        /// - 이미 장착된 아이템이 있다면 out으로 반환.
        /// </summary>
        /// <param name="type">장비 타입</param>
        /// <param name="item">착용할 아이템</param>
        /// <param name="previous">기존에 착용 중이던 아이템(out)</param>
        /// <returns>장착 성공 여부</returns>
        public bool TryEquip(Define.EquipmentType type, InventoryItem item, out InventoryItem prev)
        {
            prev = InventoryItem.GetEmptyItem();

            if (item.IsEmpty) return false;
            if (item.item is not EquippableItemSO eq) return false;
            if (eq.EquipmentType != type) return false;

            prev = _equipmentSlots[type];

            _equipmentSlots[type] = item;

            OnEquipmentSlotChanged?.Invoke(type, _equipmentSlots[type]);
            return true;
        }

        public InventoryItem Equip(Define.EquipmentType type, InventoryItem item)
        {
            TryEquip(type, item, out var previous);
            return previous;
        }

        /// <summary>
        /// 지정된 타입의 장비를 해제한다.
        /// </summary>
        /// <returns>
        /// 해제된 아이템 (없으면 Empty)
        /// </returns>
        public InventoryItem UnEquip(Define.EquipmentType type)
        {
            var unequipped = _equipmentSlots[type]; 
            if (unequipped.IsEmpty)
                return InventoryItem.GetEmptyItem();

            _equipmentSlots[type] = InventoryItem.GetEmptyItem();
            OnEquipmentSlotChanged?.Invoke(type, InventoryItem.GetEmptyItem());
            return unequipped;
        }
        #endregion

        #region 슬롯 교체 및 검증 =============================================================================

        /// <summary>
        /// 인덱스 기반으로 슬롯 아이템을 설정한다.
        /// (UI 드래그앤드랍 시 사용)
        /// </summary>
        public void SetItemAt(int index, InventoryItem item)
        {
            if (!IsValidIndex(index)) return;
            var type = SlotOrder[index];

            if (!CanAccept(item, index))
            {
                Debug.LogWarning($"[{type}] 슬롯에 [{item.item?.name}] 아이템을 장착할 수 없습니다.");
                return;
            }


            _equipmentSlots[type] = item;
            OnEquipmentSlotChanged?.Invoke(type, item);
        }

        /// <summary>
        /// 지정된 인덱스 슬롯에 주어진 아이템을 장착할 수 있는지 검사한다.
        /// - 타입 불일치 시 false
        /// - 빈 아이템은 항상 true
        public bool CanAccept(InventoryItem item, int index = -1)
        {
            if (!IsValidIndex(index)) 
                return false;
            if (item.IsEmpty) 
                return true;
            if (item.item is not EquippableItemSO eq) 
                return false;

            var type = SlotOrder[index];
            return eq.EquipmentType == type;
        }

        #endregion

        
    }
}
