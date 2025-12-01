using UnityEngine;
using static Define;

namespace Kaco.UI.Inventory
{
    public class UI_EquipSlot : UI_SlotBase
    {
        [SerializeField] Sprite _slotSprite;

        [field: SerializeField]
        public Define.EquipmentType EquipmentType { get; private set; }

        public override void Init()
        {
            base.Init();
        }

        public void SetEquipmentType(Define.EquipmentType type)
        {
            EquipmentType = type;
        }

        public override void SetData(InventoryItem item)
        {
            base.SetData(item);

            if (item.IsEmpty)
            {
                Icon.sprite = _slotSprite;
                BorderImage.enabled = false;
            }
            else
            {
                Icon.sprite = item.item.Icon;
                BorderImage.enabled = false;
            }
        }
        public EquipmentType GetEquipmentType()
        {
            return EquipmentType;
        }

    }
}
