using Kaco.UI.Inventory;
using UnityEngine;

public abstract class ItemDropHandlerSO : ScriptableObject
{
    public abstract bool CanHandle(UI_SlotBase source, UI_SlotBase destination);
    public abstract void HandleDrop(IInventoryService service, UI_SlotBase sourceSlot, UI_SlotBase destinationSlot);
}
