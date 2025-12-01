using Kaco.UI.Inventory;
using UnityEngine;
[CreateAssetMenu(fileName = "Handler_InvToEquip", menuName = "Inventory/Drop Handlers/Inventory To Equipment")]
public class InvtoEquipHandlerSO : ItemDropHandlerSO
{
    public override bool CanHandle(UI_SlotBase sourceSlot, UI_SlotBase destinationSlot)
    {
        return sourceSlot is UI_ItemSlot && destinationSlot is UI_EquipSlot;
    }

    public override void HandleDrop(IInventoryService service, UI_SlotBase sourceSlot, UI_SlotBase destinationSlot)
    {
        if (sourceSlot is not UI_ItemSlot sourceItemSlot ||
            destinationSlot is not UI_EquipSlot destEquipSlot)
        {
            return;
        }

        var invContainer = (sourceSlot.OwnerContainer as IItemContainer<InventorySO>).GetContainerData();
        var equipContainer = (destinationSlot.OwnerContainer as IItemContainer<EquipmentSO>).GetContainerData();

        // IInventoryService¿« EquipAndSwap »£√‚
        service.EquipAndSwap(
            invContainer,
            sourceItemSlot.Index,
            equipContainer,
            destEquipSlot.EquipmentType
        );
    }
}
