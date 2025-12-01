using Kaco.UI.Inventory;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kaco.UI.Inventory
{
    public class UI_Chest : UI_InteractionPopup, IItemContainer<InventorySO>
    {
        [Header("Slot Management")]
        [SerializeField] private SlotContainer _slotContainer;

        private InventorySO _inventoryData;
        private IInventoryMediatorListener _mediator;

        public override void Init()
        {
            base.Init(); // UI_Panel, UI_Popup의 Init 로직 실행
        }

        public void Initialize(InventorySO data, IInventoryMediatorListener mediator)
        {
            this._inventoryData = data;
            this._mediator = mediator;

            // SlotContainer가 슬롯을 생성하고 이벤트를 구독하도록 초기화
            if (_slotContainer != null)
            {
                // 'this'가 IItemContainer<InventorySO>를 구현하므로 전달
                _slotContainer.Initialize(data, this);
            }

            // 데이터 변경 시 UI가 자동 갱신되도록 이벤트 구독
            if (_inventoryData != null)
            {
                _inventoryData.OnInventoryUpdated -= _slotContainer.UpdateSlotsUI; // 중복 방지
                _inventoryData.OnInventoryUpdated += _slotContainer.UpdateSlotsUI;
            }
        }

        public InventorySO GetContainerData()
        {
            return _inventoryData;
        }

        public ScriptableObject GetContainerDataAsObject()
        {
            return _inventoryData;
        }

        public void HandleSlotBeginDrag(PointerEventData eventData, UI_SlotBase slot)
        {
            _mediator?.OnBeginDrag(slot);
        }

        public void HandleSlotClick(PointerEventData eventData, UI_SlotBase slot)
        {
            if (eventData.button == PointerEventData.InputButton.Right) return;
            _mediator?.OnLeftClick(slot);
        }

        public void HandleSlotDrop(PointerEventData eventData, UI_SlotBase slot)
        {
            _mediator?.OnDrop(slot);
        }

        public void HandleSlotEndDrag(PointerEventData eventData, UI_SlotBase slot)
        {
            _mediator?.OnEndDrag();
        }

        private void OnDestroy()
        {
            if (_inventoryData != null)
            {
                _inventoryData.OnInventoryUpdated -= _slotContainer.UpdateSlotsUI;
            }
        }

    }

    }

