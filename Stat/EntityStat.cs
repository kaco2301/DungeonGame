using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityStat : MonoBehaviour
{
    [SerializeField] protected Stat _baseStats = new Stat();
    protected List<StatModifier> _modifiers = new List<StatModifier>();

    public StatResult CurrentResult { get; private set; }
    public Stat CurrentStats => CurrentResult != null ? CurrentResult.FinalStat : _baseStats;
    public event Action<StatResult> OnStatsUpdated;

    protected virtual void Awake()
    {
        InitializeStats();
    }

    protected virtual void Start()
    {

        UpdateStats();
    }

    protected abstract void InitializeStats();

    public void AddModifier(StatModifier mod)
    {
        _modifiers.Add(mod);
        UpdateStats(); // 즉시 재계산 -> 새 인스턴스 생성
    }

    public void RemoveModifier(object source)
    {
        _modifiers.RemoveAll(x => x.source == source);
        UpdateStats();
    }

    // ★ 핵심: 스탯 재계산 및 인스턴스 교체
    protected void UpdateStats()
    {
        // Calculator에게 "재료"를 주고 "완제품"을 받음
        StatResult result = StatCalculator.Calculate(_baseStats, _modifiers);
        CalculateCustomStats(result.FinalStat);

        // 교체
        CurrentResult = result;

        // 알림
        OnStatsUpdated?.Invoke(CurrentResult);
    }

    /// <summary>
    /// 기본 계산(Calculator) 이후, 자식 클래스에서 파생 스탯을 추가로 계산하고 싶을 때 오버라이드
    /// </summary>
    protected virtual void CalculateCustomStats(Stat stat) { }

}
