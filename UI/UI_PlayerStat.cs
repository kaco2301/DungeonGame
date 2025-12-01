using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Define;

public class UI_PlayerStat : UI_Base
{
    [Header("Listening Channels")]
    [SerializeField] private StatEventChannelSO _statChannel;       // ╫╨ех ╪Ж╫е©К
    [SerializeField] private FloatEventChannelSO _combatPowerChannel;

    private Dictionary<PrimaryStats, TMP_Text> statTextDict = new Dictionary<PrimaryStats, TMP_Text>(); 
    private Dictionary<PrimaryStats, UI_StatTooltipTrigger> statTooltipDict = new Dictionary<PrimaryStats, UI_StatTooltipTrigger>();

    [Header("Derived Stats Texts")]
    [SerializeField] private TMP_Text combatPowerText;

    public override void Init()
    {
        BindChildren();
    }

    private void OnEnable()
    {
        if (_statChannel != null)
            _statChannel.Register(UpdateStatUI);

        if (_combatPowerChannel != null)
            _combatPowerChannel.Register(UpdateCombatPowerUI);
    }

    private void OnDisable()
    {
        if (_statChannel != null)
            _statChannel.Unregister(UpdateStatUI);

        if (_combatPowerChannel != null)
            _combatPowerChannel.Unregister(UpdateCombatPowerUI);
    }

    private void UpdateCombatPowerUI(float newCombatPower)
    {
        if (combatPowerText)
        {
            combatPowerText.text = $"{newCombatPower:F1}";
        }
    }

    private void BindChildren()
    {
        foreach (PrimaryStats type in Enum.GetValues(typeof(PrimaryStats)))
        {
            string parentName = type.ToString();
            GameObject parentGO = Util.FindChild(gameObject, parentName, true); //

            if (parentGO != null)
            {
                TMP_Text valueText = Util.FindChild<TMP_Text>(parentGO, "Value");
                if (valueText != null)
                {
                    statTextDict[type] = valueText;
                }

                UI_StatTooltipTrigger trigger = parentGO.GetComponentInChildren<UI_StatTooltipTrigger>();
                if (trigger != null)
                {
                    statTooltipDict[type] = trigger;
                }
            }
        }
    }

    private void UpdateStatUI(StatResult result)
    {
        if (result == null) return;
        Stat finalStat = result.FinalStat;

        SetText(PrimaryStats.Strength, finalStat.Strength);
        SetText(PrimaryStats.Dexterity, finalStat.Dexterity);
        SetText(PrimaryStats.Intelligence, finalStat.Intelligence);
        SetText(PrimaryStats.Vitality, finalStat.Vitality);
        SetText(PrimaryStats.Spirit, finalStat.Spirit);

        foreach (var trigger in statTooltipDict.Values)
        {
            trigger.UpdateRuntimeStats(result);
        }
    }

    private void SetText(PrimaryStats type, int value)
    {
        if (statTextDict.TryGetValue(type, out TMP_Text textComp))
        {
            textComp.text = value.ToString();
        }
    }


}
