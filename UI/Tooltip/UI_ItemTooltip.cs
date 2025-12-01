using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaco.UI.Inventory
{
    public class UI_ItemTooltip : UI_Panel, ITooltipUI
    {
        public Type HandledDataType => typeof(InventoryItem); 
        protected enum Images
        {
            ItemIcon,
            Background
        }

        protected enum Texts
        {
            ItemName,
            ItemRarity,
            Description
        }

        protected enum GameObjects
        {
            Equipped,
            ModifiersGroup
        }

        private Image ItemIcon => GetImage((int)Images.ItemIcon);
        private Image Background => GetImage((int)Images.Background);
        private TMP_Text ItemName => Get<TMP_Text>((int)Texts.ItemName);
        private TMP_Text ItemRarity => Get<TMP_Text>((int)Texts.ItemRarity);
        private TMP_Text Description => Get<TMP_Text>((int)Texts.Description);
        private GameObject Equipped => GetObject((int)GameObjects.Equipped);
        private GameObject ModifiersGroup => GetObject((int)GameObjects.ModifiersGroup);

        private List<UI_ModifierRow> _modifierRows = new List<UI_ModifierRow>();

        public override void Init()
        {
            base.Init();
            Bind<Image>(typeof(Images));
            Bind<TMP_Text>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            ModifiersGroup.GetComponentsInChildren<UI_ModifierRow>(true, _modifierRows);
            
        }

        private void Start()
        {
            Hide();
            ResetInfo();
        }

        public void SetData(object data)
        {
            if(data is InventoryItem item)
            {
                SetItemInfo(item);
            }
            else
            {
                HideTooltip();
            }
        }

        public void SetItemInfo(InventoryItem item)
        {
            if (item.IsEmpty)
            {
                HideTooltip();
                return;
            }

            if (ItemIcon != null) ItemIcon.sprite = item.item.Icon;
            if (ItemName != null) ItemName.text = item.item.Name;
            if (Description != null) Description.text = item.item.Description;
            if (ItemRarity != null)
            {
                Define.ItemRarity rarity = item.item.Rarity;
                ItemRarity.text = rarity.ToString(); 
                ItemRarity.color = GetRarityColor(rarity);
                Background.color = GetRarityColor(rarity);
            } 

            int modifierCount = 0;

            if (item.item is IHasDisplayableModifiers displayableItem)
            {
                List<DisplayableModifier> mods = displayableItem.GetModifiersForDisplay();
                modifierCount = mods.Count;

                for (int i = 0; i < modifierCount; i++)
                {
                    if (i >= _modifierRows.Count) break;

                    var modData = mods[i];
                    var rowScript = _modifierRows[i];

                    if (rowScript == null) continue;

                    // 3. [수정] 공용 struct의 데이터로 텍스트 설정
                    rowScript.SetData(modData.Icon, modData.DisplayText);
                }
            }

            for (int i = modifierCount; i < _modifierRows.Count; i++)
            {
                if (i < _modifierRows.Count && _modifierRows[i] != null)
                {
                    _modifierRows[i].ResetData();
                }
            }
        }

        private void ResetInfo()
        {
            if (ItemIcon != null) ItemIcon.sprite = null;
            if (ItemName != null) ItemName.text = "";
            if (Description != null) Description.text = "";
            if (ItemRarity != null) ItemRarity.text = "";
            if (Equipped != null) Equipped.SetActive(false); 
            if (_modifierRows != null)
            {
                foreach (var row in _modifierRows)
                {
                    row.ResetData();
                }
            }
        }

        public void ShowTooltip(Vector2 screenPosition)
        {
            transform.position = screenPosition;
            base.Show();
        }

        public void HideTooltip()
        {
            base.Hide();
            ResetInfo();
        }

        private Color GetRarityColor(Define.ItemRarity rarity)
        {
            switch (rarity)
            {
                case Define.ItemRarity.Common: return Color.gray;
                case Define.ItemRarity.Rare: return Color.cyan;
                case Define.ItemRarity.Unique: return Color.yellow;
                case Define.ItemRarity.Epic: return new Color(0.5f, 0f, 1f); // Purple
                case Define.ItemRarity.Legendary: return new Color(1f, 0.5f, 0f); // Orange
                default: return new Color32(12,86,157,240);
            }
        }

    }
}
