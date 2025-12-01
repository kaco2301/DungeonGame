using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Primary Stats(힘, 민첩 등)를 기반으로
/// Derived Stats(공격력, 회피율 등)를 계산하는 중앙 서비스
/// 모든 스탯 계산 공식은 이 클래스에 정의
/// </summary>
/// 

// 개별 스탯의 계산 상세 내역
public class StatBreakdown
{
    public float BaseValue;
    public float PrimaryBonus;      // 예: 힘으로 인한 공격력 증가분
    public float FlatModifier;      // 예: 공격력 +10 (아이템)
    public float PercentAddModifier; // 예: 공격력 +10% (패시브)
    public float PercentMultModifier = 1f; // ★ 중요: 곱연산 기본값은 1이어야 함!

    public float FinalValue =>
        (BaseValue + PrimaryBonus + FlatModifier)
        * (1f + PercentAddModifier)
        * PercentMultModifier;
}

// 계산 결과 (최종 스탯 + 상세 내역)
public class StatResult
{
    public Stat FinalStat;  // 게임 로직(이동, 데미지)에서 쓸 가벼운 객체
    public Dictionary<StatType, StatBreakdown> Breakdown; // 툴팁에서 쓸 상세 정보

    public StatResult(Stat finalStat, Dictionary<StatType, StatBreakdown> breakdown)
    {
        FinalStat = finalStat;
        Breakdown = breakdown;
    }
}

public static class StatFormulas
{
    // 힘 1당 물리 데미지
    public const float STR_DMG_MOD = 0.01f;

    // 손재주 1당 공속 / 행동속도 / 이동속도
    public const float DEX_ATK_SPD_MOD = 0.01f;
    public const float DEX_ACTION_SPD_MOD = 0.01f;
    public const float DEX_SPEED_MOD = 0.02f;

    // 지능(의지) 1당 주문력 / 마나
    public const float INT_SPELL_MOD = 0.8f;
    public const float INT_MANA_MOD = 10.0f;

    // 활력 1당 체력
    public const float VIT_HP_MOD = 5.0f;

}

public static class StatCalculator
{ 
    public static StatResult Calculate(Stat baseStat, List<StatModifier> modifiers)
    {
        var breakdown = new Dictionary<StatType, StatBreakdown>();

        // ---------------------------------------------------------
        // 1. Primary Stats 계산 (힘, 민첩 ...)
        // ---------------------------------------------------------
        float str = CalculateSingleStat(StatType.Strength, baseStat.Strength, 0, modifiers, breakdown);
        float dex = CalculateSingleStat(StatType.Dexterity, baseStat.Dexterity, 0, modifiers, breakdown);
        float @int = CalculateSingleStat(StatType.Intelligence, baseStat.Intelligence, 0, modifiers, breakdown);
        float vit = CalculateSingleStat(StatType.Vitality, baseStat.Vitality, 0, modifiers, breakdown);
        float spi = CalculateSingleStat(StatType.Spirit, baseStat.Spirit, 0, modifiers, breakdown);

        // ---------------------------------------------------------
        // 2. Derived Stats 계산 (체력, 마나, 공격력 ...)
        // ---------------------------------------------------------

        // (A) 스탯으로 인한 보너스 계산 (PrimaryBonus)
        float hpBonus = vit * StatFormulas.VIT_HP_MOD;
        float manaBonus = @int * StatFormulas.INT_MANA_MOD;

        float pDmgBonus = str * StatFormulas.STR_DMG_MOD;
        float sDmgBonus = @int * StatFormulas.INT_SPELL_MOD;

        float moveSpdBonus = dex * StatFormulas.DEX_SPEED_MOD;
        float atkSpdBonus = dex * StatFormulas.DEX_ATK_SPD_MOD;
        float actSpdBonus = dex * StatFormulas.DEX_ACTION_SPD_MOD;

        // (B) 최종 계산 (Base + PrimaryBonus + Modifiers)
        float maxHp = CalculateSingleStat(StatType.MaxHp, baseStat.MaxHp, hpBonus, modifiers, breakdown);
        float maxMana = CalculateSingleStat(StatType.MaxMana, baseStat.MaxMana, manaBonus, modifiers, breakdown);
        
        float pDmg = CalculateSingleStat(StatType.PhysicalDmgBonus, baseStat.PhysicalDmgBonus, pDmgBonus, modifiers, breakdown);
        float sDmg = CalculateSingleStat(StatType.SpellDmgBonus, baseStat.SpellDmgBonus, sDmgBonus, modifiers, breakdown);
        
        float def = CalculateSingleStat(StatType.Defense, baseStat.Defense, 0, modifiers, breakdown);
        
        float moveSpd = CalculateSingleStat(StatType.MoveSpeed, baseStat.MoveSpeed, moveSpdBonus, modifiers, breakdown);
        float atkSpd = CalculateSingleStat(StatType.AttackSpeed, baseStat.AttackSpeed, atkSpdBonus, modifiers, breakdown);
        float actSpd = CalculateSingleStat(StatType.ActionSpeed, baseStat.ActionSpeed, actSpdBonus, modifiers, breakdown);

        // ---------------------------------------------------------
        // 3. 결과 포장
        // ---------------------------------------------------------
        Stat finalStat = new Stat(
            (int)str, (int)dex, (int)@int, (int)vit, (int)spi,
            baseStat.Level,
            maxHp, maxMana, pDmg, sDmg, def, moveSpd, atkSpd, actSpd
        );

        return new StatResult(finalStat, breakdown);
    }

    private static float CalculateSingleStat(StatType type, float baseVal, float primaryBonus, List<StatModifier> mods, Dictionary<StatType, StatBreakdown> breakdown)
    {
        StatBreakdown data = new StatBreakdown
        {
            BaseValue = baseVal,
            PrimaryBonus = primaryBonus,
            PercentMultModifier = 1f // 곱연산 초기값은 반드시 1
        };


        // Modifier 순회
        foreach (var mod in mods)
        {
            if (mod.statType == type)
            {
                switch (mod.modType)
                {
                    case StatModType.Flat:
                        data.FlatModifier += mod.value;
                        break;
                    case StatModType.PercentAdd:
                        data.PercentAddModifier += mod.value;
                        break;
                    case StatModType.PercentMult:
                        data.PercentMultModifier *= mod.value;
                        break;
                }
            }
        }

        breakdown[type] = data; // 딕셔너리에 저장
        return data.FinalValue; // 최종값 반환
    }
}
