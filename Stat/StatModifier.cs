using UnityEngine;

public enum StatType
{
    Strength, Dexterity, Intelligence, Vitality, Spirit,
    MaxHp, MaxMana, Defense, PhysicalDmgBonus, SpellDmgBonus,
    MoveSpeed, AttackSpeed, ActionSpeed
}

public enum StatModType
{
    Flat,       // 합연산 (+10)
    PercentAdd, // 합연산 퍼센트 (+10%)
    PercentMult // 곱연산 퍼센트 (*1.1)
}

[System.Serializable]
public class StatModifier
{
    public StatType statType;
    public StatModType modType;
    public float value;
    public object source; // 디버깅용 (누가 이 버프를 걸었나?)

    public StatModifier(StatType type, StatModType modType, float value, object source)
    {
        this.statType = type;
        this.modType = modType;
        this.value = value;
        this.source = source;
    }
}
