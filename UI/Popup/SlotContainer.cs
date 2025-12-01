using Kaco.UI.Inventory;
using System;
using System.Collections.Generic;
using UnityEngine;

//슬롯의 생명주기 관리.
public class SlotContainer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private UI_ItemSlot slotPrefab;

    private InventorySO _inventoryData;
    private List<UI_ItemSlot> _slots = new List<UI_ItemSlot>();
    private IItemContainer _ownerPanel;

    public InventorySO GetInventorySO() => _inventoryData;

    public void Initialize(InventorySO inventoryData, IItemContainer owner)
    {
        Debug.Log("Initialize");
        if (this._inventoryData != null)
        {
            this._inventoryData.OnInventoryUpdated -= UpdateSlotsUI;
        }

        foreach (var slot in _slots)
        {
            Destroy(slot.gameObject);
        }
        _slots.Clear();

        this._inventoryData = inventoryData;
        this._ownerPanel = owner;

        if (this._inventoryData == null) return;

        CreateSlots();

        UpdateSlotsUI(_inventoryData.GetCurrentInventoryState());
    }



    private void CreateSlots()
    {
        Debug.Log("createslot");
        for (int i = 0; i < _inventoryData.Size; i++)
        {
            var newSlot = Instantiate(slotPrefab, this.transform);
            newSlot.Initialize(_ownerPanel);
            newSlot.SetIndex(i);
            _slots.Add(newSlot);
        }
    }

    public void UpdateSlotsUI(Dictionary<int, InventoryItem> inventoryState)
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (inventoryState.TryGetValue(i, out InventoryItem item))
            {
                _slots[i].SetData(item);
            }
            else
            {
                _slots[i].ResetData();
            }
        }
    }

    // 오브젝트가 파괴될 때 이벤트 구독 해제 (메모리 누수 방지)
    private void OnDestroy()
    {
        if (_inventoryData != null)
        {
            _inventoryData.OnInventoryUpdated -= UpdateSlotsUI;
        }
    }
}
