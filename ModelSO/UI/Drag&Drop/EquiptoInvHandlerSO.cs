using Kaco.UI.Inventory;
using UnityEngine;
[CreateAssetMenu(fileName = "Handler_EquipToInv", menuName = "Inventory/Drop Handlers/Equipment To Inventory")]
public class EquiptoInvHandlerSO : ItemDropHandlerSO
{
    public override bool CanHandle(UI_SlotBase sourceSlot, UI_SlotBase destinationSlot)
    {
        return sourceSlot is UI_EquipSlot && destinationSlot is UI_ItemSlot;
    }

    public override void HandleDrop(IInventoryService service, UI_SlotBase sourceSlot, UI_SlotBase destinationSlot)
    {
        if (sourceSlot is not UI_EquipSlot sourceEquipSlot ||
            destinationSlot is not UI_ItemSlot destItemSlot)
        {
            return;
        }

        var equipContainer = (sourceSlot.OwnerContainer as IItemContainer<EquipmentSO>).GetContainerData();
        var invContainer = (destinationSlot.OwnerContainer as IItemContainer<InventorySO>).GetContainerData();

        // IInventoryService¿« UnequipToSlot »£√‚
        service.MoveItem(
            equipContainer, 
            equipContainer.GetIndexOfType(sourceEquipSlot.EquipmentType),
            invContainer,
            destItemSlot.Index
        );
    }
}
