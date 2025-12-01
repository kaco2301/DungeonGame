using Kaco.UI.Inventory;
using UnityEngine;
[CreateAssetMenu(fileName = "Handler_InvToInv", menuName = "Inventory/Drop Handlers/Inventory To Inventory")]
public class InvtoInvHandlerSO : ItemDropHandlerSO
{
    public override bool CanHandle(UI_SlotBase sourceSlot, UI_SlotBase destinationSlot)
    {
        return sourceSlot is UI_ItemSlot && destinationSlot is UI_ItemSlot;
    }

    public override void HandleDrop(IInventoryService service, UI_SlotBase sourceSlot, UI_SlotBase destinationSlot)
    {
        if (sourceSlot is not UI_ItemSlot sourceItemSlot ||
            destinationSlot is not UI_ItemSlot destItemSlot)
        {
            return;
        }

        // IItemContainer<T>는 GetContainerData()를 보장함
        var sourceStorage = (sourceSlot.OwnerContainer as IItemContainer<InventorySO>).GetContainerData();
        var destStorage = (destinationSlot.OwnerContainer as IItemContainer<InventorySO>).GetContainerData();

        // IInventoryService의 MoveItem 호출
        service.MoveItem(
            sourceStorage,
            sourceItemSlot.Index,
            destStorage,
            destItemSlot.Index
        );
    }

}
