using TMPro;

//  역할: 하나의 장비 슬롯 시각적 표현
//  위치: UI Canvas의 자식 GameObject
//  생명주기: UI가 활성화된 동안
//  단일 책임: 하나의 슬롯만 담당
//  담당: 드래그앤드롭 감지
namespace Kaco.UI.Inventory
{
    public class UI_ItemSlot : UI_SlotBase
    {
        protected enum Texts
        {
            QuantityTxt
        }

        protected TMP_Text QuantityTxt => Get<TMP_Text>((int)Texts.QuantityTxt);

        public int Index { get; private set; }

        public override void Init()
        {
            base.Init();
            Bind<TMP_Text>(typeof(Texts));
        }

        public override void SetData(InventoryItem item)
        {
            base.SetData(item);
            if (item.IsEmpty)
            {
                Icon.gameObject.SetActive(false);
                BorderImage.enabled = false;
                QuantityTxt.text = "";
            }
            else
            {
                Icon.gameObject.SetActive(true);
                Icon.sprite = item.item.Icon; 
                BorderImage.enabled = false;
                QuantityTxt.text = item.quantity > 1 ? item.quantity.ToString() : "";
            }
        }

        public void SetIndex(int index)
        {
            this.Index = index;
        }

        public int GetIndex()
        {
            return this.Index;
        }

    }
}
