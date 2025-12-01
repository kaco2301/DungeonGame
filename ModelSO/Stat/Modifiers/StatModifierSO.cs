using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Stats/Stat Modifier Data")]
public class StatModifierSO : ScriptableObject
{
    [Header("UI Display Info")]
    public Sprite Icon; // 툴팁에 표시할 아이콘
    public string StatName; //툴팁에 표시할 이름
    
    [Header("Configuration")]
    [Tooltip("어떤 스탯을 건드릴 것인가?")]
    public StatType TargetStat;

    [Tooltip("어떻게 계산할 것인가? (고정합/퍼센트)")]
    public StatModType ModifierType;

    /// <summary>
    /// 실제 계산에 쓰일 데이터 객체(StatModifier)를 생성해서 반환
    /// </summary>
    public StatModifier CreateModifier(float value, object source)
    {
        float finalValue = value;

        switch (ModifierType)
        {
            case StatModType.PercentAdd:
                finalValue = value / 100f;  // 입력: 10 → 저장: 0.10
                break;

            case StatModType.PercentMult:
                if (value <= 0f)
                    Debug.LogWarning($"{name} PercentMult 값이 0 이하입니다. 입력값 확인 필요.");
                break;
        }


        // 아까 정의한 순수 데이터 클래스(StatModifier)를 생성
        return new StatModifier(TargetStat, ModifierType, finalValue, source);
    }
}

[Serializable]
public class ModifierData
{
    public StatModifierSO statModifier;
    public float value;

    public StatModifier CreateRuntimeModifier(object source)
    {
        if (statModifier == null) return default;
        return statModifier.CreateModifier(value, source);
    }
}
