using Kaco.UI.Inventory;
using UnityEngine;

public enum InventoryError
{
    None,
    SourceEmpty,
    DestinationFull,
    TypeMismatch,
    InvalidIndex,
    EquipmentSlotOccupied, 
    UnknownContainer
}

public struct InventoryResult
{
    public bool Success => Error == InventoryError.None;
    public InventoryError Error;

    public static InventoryResult Ok() => new() { Error = InventoryError.None };
    public static InventoryResult Fail(InventoryError error) => new() { Error = error };
}

public interface IItemStorage
{
    int Capacity { get; }
    InventoryItem GetItemAt(int index);
    void SetItemAt(int index, InventoryItem item);
    bool IsValidIndex(int index); 
    bool CanAccept(InventoryItem item, int index = -1);// ��: ��� ���� Ÿ�� ����                     
}

public interface IInventoryService
{
    InventoryResult MoveItem(IItemStorage source, int sourceIndex, IItemStorage dest, int destIndex);
    InventoryResult EquipAndSwap(IInventoryStorage inventory, int invIndex, IItemStorage equipment, Define.EquipmentType equipType);
    InventoryResult UnequipToSlot(IItemStorage equipment, Define.EquipmentType equipType, IInventoryStorage inventory, int invIndex);
    InventoryResult UnequipToInventory(IItemStorage equipment, Define.EquipmentType equipType, IInventoryStorage inventory);
}

public class InventoryService : IInventoryService
{
    
    public InventoryResult MoveItem(IItemStorage source, int sourceIndex, IItemStorage dest, int destIndex)
    {
        if (source == null || dest == null)
            return InventoryResult.Fail(InventoryError.UnknownContainer);

        if (!source.IsValidIndex(sourceIndex) || !dest.IsValidIndex(destIndex))
            return InventoryResult.Fail(InventoryError.InvalidIndex);

        InventoryItem sourceItem = source.GetItemAt(sourceIndex);
        InventoryItem destItem = dest.GetItemAt(destIndex);

        if (sourceItem.IsEmpty)
            return InventoryResult.Fail(InventoryError.SourceEmpty);

        if (!dest.CanAccept(sourceItem, destIndex))
            return InventoryResult.Fail(InventoryError.TypeMismatch);

        if (!source.CanAccept(destItem, sourceIndex))
            return InventoryResult.Fail(InventoryError.TypeMismatch);

        // 6. �±�ȯ
        source.SetItemAt(sourceIndex, destItem);
        dest.SetItemAt(destIndex, sourceItem);

        return InventoryResult.Ok();
    }

    public InventoryResult EquipAndSwap(IInventoryStorage inventory, int invIndex, IItemStorage equipment, Define.EquipmentType equipType)
    {
        if (inventory is not IInventoryStorage invStorage || equipment is not IEquipmentStorage equipStorage)
            return InventoryResult.Fail(InventoryError.UnknownContainer);

        InventoryItem itemToEquip = invStorage.GetItemAt(invIndex);
        if (itemToEquip.IsEmpty)
            return InventoryResult.Fail(InventoryError.SourceEmpty);

        if (itemToEquip.item is not EquippableItemSO equippable)
            return InventoryResult.Fail(InventoryError.TypeMismatch);

        if (equippable.EquipmentType != equipType)
            return InventoryResult.Fail(InventoryError.TypeMismatch);

        InventoryItem oldItem = equipStorage.Equip(equipType, itemToEquip);
        invStorage.SetItemAt(invIndex, oldItem);

        return InventoryResult.Ok();
    }

    public InventoryResult UnequipToSlot(IItemStorage equipment, Define.EquipmentType equipType, IInventoryStorage inventory, int invIndex)
    {
        if (equipment is not IEquipmentStorage equipStorage || inventory is not IItemStorage invStorage)
            return InventoryResult.Fail(InventoryError.UnknownContainer);

        InventoryItem itemToUnequip = equipStorage.GetItemAt(equipType);
        if (itemToUnequip.IsEmpty)
            return InventoryResult.Fail(InventoryError.SourceEmpty);

        InventoryItem itemAtDest = invStorage.GetItemAt(invIndex);

        if (invStorage.CanAccept(itemToUnequip, invIndex))
        {
            // 6. [����] �������̽��� �޼��� ȣ��
            equipStorage.Equip(equipType, itemAtDest);
            invStorage.SetItemAt(invIndex, itemToUnequip);
            return InventoryResult.Ok();
        }

        return InventoryResult.Fail(InventoryError.EquipmentSlotOccupied);
    }

    public InventoryResult UnequipToInventory(IItemStorage equipment, Define.EquipmentType equipType, IInventoryStorage inventory)
    {
        if (equipment is not IEquipmentStorage equipStorage || inventory is not IInventoryStorage invStorage)
            return InventoryResult.Fail(InventoryError.UnknownContainer);

        InventoryItem itemToUnequip = equipStorage.GetItemAt(equipType);
        if (itemToUnequip.IsEmpty)
            return InventoryResult.Fail(InventoryError.SourceEmpty);

        int remainder = invStorage.AddItem(itemToUnequip.item, itemToUnequip.quantity, itemToUnequip.itemState);

        if (remainder == 0)
        {
            equipStorage.UnEquip(equipType);
            return InventoryResult.Ok();
        }

        return InventoryResult.Fail(InventoryError.DestinationFull);
    }
    
}