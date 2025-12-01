using Kaco.UI.Inventory;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 모든 아이템 UI 패널 간의 상호작용을 중재하는 중앙 '중재자'(Mediator)입니다.
/// IInventoryMediatorListener를 구현하여 패널로부터 이벤트를 수신합니다.
/// OnDrop 시 어떤 로직(이동, 장착, 해제)을 실행할지 결정하고 InventoryService를 호출합니다.
/// 아이템 우클릭 등 플레이어의 입력을 처리하여 InventoryController에 액션을 위임합니다.
/// </summary>
/// 
public interface IInventoryMediatorListener
{
    void OnBeginDrag(UI_SlotBase sourceSlot);
    void OnDrop(UI_SlotBase destinationSlot);
    void OnEndDrag();
    void OnLeftClick(UI_SlotBase slot);
}

//조정과 조율
public class InventoryMediator : MonoBehaviour, IInventoryMediatorListener
{
    [Header("Logic & System References")]
    [SerializeField] private List<ItemDropHandlerSO> _dropHandlers; 
    
    private IInventoryService _inventoryService;

    private UI_SlotBase _sourceSlot; 
    private UI_SlotBase _currentlySelectedSlot;
    private bool _wasDroppedOnSlot = false;

    public void Initialize(IInventoryService service)
    {
        _inventoryService = service;
    }

    /// <summary>
    /// ItemPanelBase가 OnChildSlotClick을 받아 호출합니다.
    /// (기존 HandleRightClick 로직)
    /// </summary>
    public void OnLeftClick(UI_SlotBase slot)
    {// 1. 빈 슬롯을 클릭한 경우
        if (slot.IsEmpty)
        {
            // 이전에 선택된 것이 있었다면 선택 해제
            if (_currentlySelectedSlot != null)
            {
                _currentlySelectedSlot.DeselectItem();
            }
            _currentlySelectedSlot = null; // 선택 없음
            return;
        }

        // 2. 아이템이 있는 슬롯을 클릭한 경우

        // 2a. 이미 선택된 슬롯을 다시 클릭한 경우 (선택 해제)
        if (_currentlySelectedSlot == slot)
        {
            slot.DeselectItem();
            _currentlySelectedSlot = null;
        }
        // 2b. 다른 슬롯을 클릭한 경우 (선택 변경)
        else
        {
            // 이전에 선택된 것이 있었다면 해제
            if (_currentlySelectedSlot != null)
            {
                _currentlySelectedSlot.DeselectItem();
            }
            // 새 슬롯을 선택
            slot.SelectItem();
            _currentlySelectedSlot = slot;
        }
    }

    public void OnBeginDrag(UI_SlotBase slot)
    {
        if (slot?.OwnerContainer == null) return;
        _sourceSlot = slot; 
        _wasDroppedOnSlot = false;
        UIManager.Instance.ShowMouseFollower(slot.AssignedItem);
    }

    /// <summary>
    /// [수정] 모든 데이터 접근을 OwnerContainer.GetContainerData()로 변경
    /// </summary>
    public void OnDrop(UI_SlotBase destinationSlot)
    {
        if (_sourceSlot == null || destinationSlot?.OwnerContainer == null || _sourceSlot == destinationSlot)
        {
            return;
        }
        _wasDroppedOnSlot = true;


        foreach (var handler in _dropHandlers)
        {
            if (handler.CanHandle(_sourceSlot, destinationSlot))
            {
                Debug.Log($"[Mediator] 핸들러 실행: {handler.name}");
                handler.HandleDrop(_inventoryService,_sourceSlot, destinationSlot);
                break;
            }
        }
    }

    public void OnEndDrag()
    {
        if (!_wasDroppedOnSlot && _sourceSlot != null)
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                HandleDropOutside(_sourceSlot);
            }
        }

        UIManager.Instance.HideMouseFollower();
        _sourceSlot = null;
    }

    public void HandleDropOutside(UI_SlotBase sourceSlot)
    {
        var sourceContainer = _sourceSlot.OwnerContainer;

        if (sourceContainer is IItemContainer<InventorySO> invContainer &&
        sourceSlot is UI_ItemSlot invSlot)
        {
            // 슬롯에서 아이템 정보를 가져옴
            var itemToDrop = invContainer.GetContainerData().GetItemAt(invSlot.Index);
            if (!itemToDrop.IsEmpty)
            {
                // InventorySO에서 해당 아이템을 (전부) 제거
                invContainer.GetContainerData().RemoveItem(invSlot.Index, itemToDrop.quantity);
                Debug.Log($"[Mediator] 아이템 버림: {itemToDrop.item.Name}");
            }
        }
        // 2. [수정] 장비창 -> 바닥 (장비 벗어서 버리기)
        else if (sourceContainer is IItemContainer<EquipmentSO> equipContainer &&
                 sourceSlot is UI_EquipSlot equipSlot)
        {
            // EquipmentSO에서 해당 장비를 그냥 제거 (인벤토리로 X)
            var itemToDrop = equipContainer.GetContainerData().UnEquip(equipSlot.EquipmentType);
            if (!itemToDrop.IsEmpty)
            {
                Debug.Log($"[Mediator] 장비 버림: {itemToDrop.item.Name}");
            }
        }
    }

    public InventoryItem GetItemFromSlot(UI_SlotBase slot)
    {
        return slot?.AssignedItem ?? InventoryItem.GetEmptyItem();
    }

}
