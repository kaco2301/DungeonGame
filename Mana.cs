using System;
using UnityEngine;

public class Mana : MonoBehaviour
{
    public event Action<float, float> OnManaChanged; // (Current, Max)

    private EntityStat _stat;
    [SerializeField] private float _currentMana;

    public float CurrentMana => _currentMana;
    public float MaxMana => _stat != null ? _stat.CurrentStats.MaxMana : 0;


    [SerializeField] private ManaEventChannelSO _manaChannel;

    // 만약 마나 자동 회복 기능이 있다면 (Update문 사용 or Coroutine)
    // private float _regenTimer; 

    private void Awake()
    {
        _stat = GetComponent<EntityStat>();
    }

    private void OnEnable()
    {
        if (_stat != null)
        {
            _stat.OnStatsUpdated += UpdateStats;
            if (_stat.CurrentStats != null)
                UpdateStats(_stat.CurrentResult);
        }

        // 시작 시 마나 풀
        // _currentMana = MaxMana; 
    }

    private void OnDisable()
    {
        if (_stat != null)
            _stat.OnStatsUpdated -= UpdateStats;
    }

    private void UpdateStats(StatResult result)
    {
        Stat stat = result.FinalStat;
        // 장비 교체로 MaxMana 줄어들었을 때 처리
        if (_currentMana > stat.MaxMana)
        {
            _currentMana = stat.MaxMana;
        }
        PublishManaEvent();
    }

    /// <summary>
    /// 스킬 사용 시 호출. 마나가 충분하면 줄이고 true 반환, 부족하면 false 반환.
    /// </summary>
    public bool UseMana(float amount)
    {
        if (_currentMana >= amount)
        {
            _currentMana -= amount;

            PublishManaEvent();
            return true;
        }

        Debug.Log("마나가 부족합니다!");
        return false;
    }

    public void RestoreMana(float amount)
    {
        _currentMana = Mathf.Clamp(_currentMana + amount, 0, MaxMana);
        PublishManaEvent();
    }

    private void PublishManaEvent()
    {
        // C# 이벤트 (기존 유지)
        OnManaChanged?.Invoke(_currentMana, MaxMana);

        // ★ [수정] 구조체(HealthData)로 포장해서 발송
        if (_manaChannel != null)
        {
            _manaChannel.RaiseEvent(new ManaData(_currentMana, MaxMana));
        }
    }
}