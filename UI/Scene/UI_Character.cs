using Kaco.UI.Inventory;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IItemContainer<T> : IItemContainer where T : ScriptableObject
{
    void Initialize(T data, IInventoryMediatorListener mediator);
    T GetContainerData();
}

public interface IItemContainer
{
    ScriptableObject GetContainerDataAsObject();
    void HandleSlotBeginDrag(PointerEventData eventData, UI_SlotBase slot);
    void HandleSlotDrop(PointerEventData eventData, UI_SlotBase slot);
    void HandleSlotEndDrag(PointerEventData eventData, UI_SlotBase slot);
    void HandleSlotClick(PointerEventData eventData, UI_SlotBase slot);
}

/// <summary>
/// 인벤토리와 장비창 UI를 모두 포함하고 관리하는 단일 패널
/// IItemContainer<InventorySO>와 IItemContainer<EquipmentSO>를 모두 구현하여
/// Mediator가 슬롯의 출처를 구분할 수 있게 합니다.
/// </summary>
public class UI_Character : UI_Scene, IItemContainer<InventorySO>, IItemContainer<EquipmentSO>
{

    [Header("Data References")]
    private InventorySO _inventoryData;
    private EquipmentSO _equipmentData;
    private IInventoryMediatorListener _mediator;

    [Header("Slots")]
    [SerializeField] private SlotContainer _slotContainer; // UI_Inventory에서 가져옴
    [SerializeField] private UI_EquipSlot[] _equipmentSlots; // Bind 대신 SerializeField로 받는 것이 더 쉬울 수 있음

    private Dictionary<Define.EquipmentType, UI_EquipSlot> _slotDict;

    public override void Init()
    {
        base.Init();

        _slotDict = new Dictionary<Define.EquipmentType, UI_EquipSlot>();

        foreach (var slot in _equipmentSlots)
        {
            if (slot != null && !_slotDict.ContainsKey(slot.EquipmentType))
            {
                _slotDict.Add(slot.EquipmentType, slot);
            }
        }
    }

    public void Initialize(InventorySO invData, EquipmentSO equipData, IInventoryMediatorListener mediator)
    {
        this._inventoryData = invData;
        this._equipmentData = equipData;
        this._mediator = mediator;

        _slotContainer.Initialize(invData, this);

        // --- UI_Equipment의 초기화 로직 ---
        foreach (var slot in _equipmentSlots)
        {
            slot.Initialize(this.GetComponent<IItemContainer>());
        }

        // 이벤트 구독
        _equipmentData.OnEquipmentSlotChanged += UpdateSingleEquipmentSlot; 
        _inventoryData.OnInventoryUpdated += _slotContainer.UpdateSlotsUI;
    }

    private void Start()
    {
        if (_equipmentData != null)
            UpdateAllEquipmentSlots(_equipmentData.GetCurrentEquipmentState());

        if (_inventoryData != null)
            _slotContainer.UpdateSlotsUI(_inventoryData.GetCurrentInventoryState());
    }

    public InventorySO GetContainerData()
    {
        return _inventoryData;
    }

    EquipmentSO IItemContainer<EquipmentSO>.GetContainerData()
    {
        return _equipmentData;
    }

    public ScriptableObject GetContainerDataAsObject()
    {
        return _inventoryData; // 기본값
    }

    //장비창 갱신
    private void UpdateSingleEquipmentSlot(Define.EquipmentType type, InventoryItem item)
    {
        if (_slotDict.TryGetValue(type, out UI_EquipSlot slot))
        {
            // 해당 슬롯의 데이터만 갱신
            if (item.IsEmpty)
            {
                slot.ResetData();
            }
            else
            {
                slot.SetData(item);
            }
        }
    }

    //모든 장비창 갱신
    private void UpdateAllEquipmentSlots(IReadOnlyDictionary<Define.EquipmentType, InventoryItem> state)
    {
        foreach (var slotEntry in _slotDict)
        {
            var type = slotEntry.Key;
            var slot = slotEntry.Value;

            if (slot != null)
            {
                if (state.TryGetValue(type, out InventoryItem item) && !item.IsEmpty)
                {
                    slot.SetData(item);
                }
                else
                {
                    slot.ResetData();
                }
            }
        }
    }

    public virtual void HandleSlotBeginDrag(PointerEventData eventData, UI_SlotBase slot)
    {
        if (slot == null) return;
        _mediator?.OnBeginDrag(slot);
    }

    public virtual void HandleSlotDrop(PointerEventData eventData, UI_SlotBase destinationSlot)
    {
        if (destinationSlot == null) return;
        _mediator?.OnDrop(destinationSlot);
    }

    public virtual void HandleSlotEndDrag(PointerEventData eventData, UI_SlotBase slot)
    {
        _mediator?.OnEndDrag();
    }

    public virtual void HandleSlotClick(PointerEventData eventData, UI_SlotBase slot)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            return;
        if (slot == null) return;

        _mediator?.OnLeftClick(slot);
    }

    public void Initialize(EquipmentSO data, IInventoryMediatorListener mediator)
    {
    }

    public void Initialize(InventorySO data, IInventoryMediatorListener mediator)
    {
    }

    private void OnDestroy()
    {
        if (_equipmentData != null)
            _equipmentData.OnEquipmentSlotChanged -= UpdateSingleEquipmentSlot;
        if (_inventoryData != null)
            _inventoryData.OnInventoryUpdated -= _slotContainer.UpdateSlotsUI;
    }
}
