using UnityEngine;
using static Define;

public class TooltipEventData
{
    public object Data { get; }
    public Vector2 ScreenPosition { get; }

    public TooltipEventData(object data, Vector2 screenPosition)
    {
        Data = data;
        ScreenPosition = screenPosition;
    }
}

public class StatTooltipData
{
    public StatDataSO Definition { get; }
    public Stat RuntimeStats { get; }
    public StatBreakdown Breakdown { get; }

    public StatTooltipData(StatDataSO def, Stat runtime, StatBreakdown breakdown)
    {
        Definition = def;
        RuntimeStats = runtime;
        Breakdown = breakdown;
    }
}

[CreateAssetMenu(fileName = "new StatData", menuName = "Stats/StatData/new StatData")]
public class StatDataSO : ScriptableObject
{
    [Header("스탯 타입")]
    public StatType statType; // (예: Strength)

    [Header("UI 표시 정보")]
    public string statName; // (예: "힘")
    public Sprite icon;
    [TextArea(3, 10)]
    public string description;
}
