using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kaco.UI.Inventory
{
    public abstract class UI_SlotBase : UI_Base
    {
        protected enum Images
        {
            Icon,
            BorderImage
        }

        public IItemContainer OwnerContainer { get; private set; }
        protected Image Icon => GetImage((int)Images.Icon);
        protected Image BorderImage => GetImage((int)Images.BorderImage);

        protected InventoryItem _assignedItem; 
        public InventoryItem AssignedItem => _assignedItem;

        [Header("Tooltip Channels")]
        [SerializeField] private TooltipEventChannelSO _tooltipShowChannel;
        [SerializeField] private VoidEventChannelSO _tooltipHideChannel;

        public bool IsEmpty => _assignedItem.IsEmpty;

        public override void Init()
        {
            Bind<Image>(typeof(Images));
        }

        public virtual void Initialize(IItemContainer owner)
        {
            this.OwnerContainer = owner;

            BindEvent(gameObject, OnBeginDrag, Define.UIEvent.BeginDrag);
            BindEvent(gameObject, OnEndDrag, Define.UIEvent.EndDrag);
            BindEvent(gameObject, OnDrop, Define.UIEvent.Drop);
            BindEvent(gameObject, OnClick, Define.UIEvent.Click); 
            BindEvent(gameObject, OnPointerEnter, Define.UIEvent.PointerEnter);
            BindEvent(gameObject, OnPointerExit, Define.UIEvent.PointerExit);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // 빈 슬롯이 아니면 툴팁 요청
            if (_assignedItem.IsEmpty) return;
            var tooltipData = new TooltipEventData(_assignedItem, (Vector2)transform.position);
            _tooltipShowChannel?.RaiseEvent(tooltipData);

        }

        // 4. [신규] 마우스가 슬롯에서 나갔을 때
        public void OnPointerExit(PointerEventData eventData)
        {
            _tooltipHideChannel?.RaiseEvent();
        }

        private void OnBeginDrag(PointerEventData eventData)
        {
            // 캐스팅 없이 'this'(슬롯 자신)를 전달
            OwnerContainer?.HandleSlotBeginDrag(eventData, this);
        }

        private void OnEndDrag(PointerEventData eventData)
        {
            OwnerContainer?.HandleSlotEndDrag(eventData, this);
        }

        private void OnDrop(PointerEventData eventData)
        {
            // 'OnDrop'은 이 슬롯(목적지)에서 발생함
            OwnerContainer?.HandleSlotDrop(eventData, this);
        }

        private void OnClick(PointerEventData eventData)
        {
            OwnerContainer?.HandleSlotClick(eventData, this);
        }

        public virtual void SetData(InventoryItem item)
        {
            _assignedItem = item;
        }
        
        public virtual void ResetData()
        {
            SetData(InventoryItem.GetEmptyItem());
        }

        public void SelectItem() => BorderImage.enabled = true;
        public void DeselectItem() => BorderImage.enabled = false;
    }

    
}


