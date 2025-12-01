using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_StatTooltip : UI_Panel, ITooltipUI
{
    protected enum Images
    {
        StatIcon
    }

    protected enum Texts
    {
        StatName,
        Description,
        DerivedStat
    }

    private Image StatIcon => Get<Image>((int)Images.StatIcon);
    private TMP_Text StatName => Get<TMP_Text>((int)Texts.StatName);
    private TMP_Text Description => Get<TMP_Text>((int)Texts.Description);
    private TMP_Text DerivedStat => Get<TMP_Text>((int)Texts.DerivedStat);

    public Type HandledDataType => typeof(StatTooltipData);
    
    
    public override void Init()
    {
        base.Init();
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
        Hide();
        ResetInfo();
    }


    public void SetData(object data)
    {
        if(data is StatTooltipData statData)
        {
            SetStatInfo(statData);
        }
        else
        {
            HideTooltip();
        }
    }

    public void SetStatInfo(StatTooltipData data)
    {

        if (data == null)
        {
            HideTooltip();
            return;
        }

        StatDataSO def = data.Definition; // '정의' (이름, 아이콘, 설명)
        Stat stats = data.RuntimeStats;   // '런타임' (계산된 보너스 값)

        if (StatIcon != null) StatIcon.sprite = def.icon;
        if (StatName != null) StatName.text = def.statName;
        if (Description != null) Description.text = def.description; 
        if (DerivedStat != null)
        {
            string text = "";
            if (data.Breakdown != null)
            {
                text += $"<color=#FFFFFF>최종 값: {data.Breakdown.FinalValue:0.##}</color>\n";
                text += $"----------------\n";
                text += $"기본: {data.Breakdown.BaseValue}\n";

                // 스탯 보너스 (조건문 삭제 -> 0이어도 표시)
                text += $"스탯 보너스: +{data.Breakdown.PrimaryBonus:0.##}\n";

                // 장비/효과 (조건문 삭제 -> 0이어도 표시)
                text += $"장비/효과: +{data.Breakdown.FlatModifier:0.##}\n";

                // 퍼센트 추가
                text += $"추가(%): +{data.Breakdown.PercentAddModifier * 100:0}%\n";

                // 곱연산 (1이 아닐 때만 표시 권장)
                if (data.Breakdown.PercentMultModifier != 1f)
                {
                    text += $"최종 증폭: x{data.Breakdown.PercentMultModifier:0.##}\n";
                }
            }

            string impactText = GetDerivedStatDescription(data.Definition.statType, data.RuntimeStats);

            if (!string.IsNullOrEmpty(impactText))
            {
                text += $"\n<color=#FFFF00>[효과]</color>\n{impactText}";
            }
            // Breakdown도 없고 효과도 없으면(단순 설명 스탯) 기본 설명 출력
            else if (data.Breakdown == null)
            {
                text = data.Definition.description;
            }

            // 3. 최종 적용
            if (Description != null) Description.text = text;
        }

    }
    private string GetDerivedStatDescription(StatType type, Stat stats)
    {
        switch (type)
        {
            case StatType.Strength:
                // 힘 1당 데미지 공식 가져오기
                float pDmg = stats.Strength * StatFormulas.STR_DMG_MOD;
                return $"• 물리 피해: +{pDmg * 100f:0.#}%"; // (0.15 -> 15%)

            case StatType.Dexterity:
                float atkSpd = stats.Dexterity * StatFormulas.DEX_ATK_SPD_MOD;
                float actSpd = stats.Dexterity * StatFormulas.DEX_ACTION_SPD_MOD;
                float moveSpd = stats.Dexterity * StatFormulas.DEX_SPEED_MOD;
                return $"• 공격 속도: +{atkSpd * 100f:0.#}%\n• 행동 속도: +{actSpd * 100f:0.#}%\n• 이동 속도: +{moveSpd:0.##}";

            case StatType.Intelligence:
                float sDmg = stats.Intelligence * StatFormulas.INT_SPELL_MOD;
                float mana = stats.Intelligence * StatFormulas.INT_MANA_MOD;
                return $"• 주문 피해: +{sDmg:0.#}\n• 최대 마나: +{mana:0}";

            case StatType.Vitality:
                float hp = stats.Vitality * StatFormulas.VIT_HP_MOD;
                return $"• 최대 체력: +{hp:0}";

            case StatType.Spirit:
                return "";

            default:
                return ""; // 파생 효과가 없는 스탯(MaxHp 등)은 빈 문자열
        }
    }
    private void ResetInfo()
    {
        if(StatIcon != null) StatIcon.sprite = null;
        if (StatName != null) StatName.text = "";
        if (Description != null) Description.text = "";
        if (DerivedStat != null) DerivedStat.text = "";
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

    
}
