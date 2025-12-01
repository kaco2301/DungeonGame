using System;
using UnityEngine;
using UnityEngine.UI;



public class Health : MonoBehaviour, IDamageable
{
    public event Action<float, float> OnHealthChanged; // (현재HP, 최대HP)
    public event Action OnDeath;

    private EntityStat _stat;

    [SerializeField] private float _currentHp;
    public float CurrentHp => _currentHp;
    public float MaxHp => _stat != null ? _stat.CurrentStats.MaxHp : 0;

    [SerializeField] private HealthEventChannelSO _healthChannel;

    private void Awake()
    {
        _stat = GetComponent<EntityStat>();
    }

    private void OnEnable()
    {
        // 이벤트 구독: 스탯이 바뀌면(장비 교체 등) 내 정보를 갱신한다.
        if (_stat != null)
        {
            _stat.OnStatsUpdated += UpdateStats;

            if (_stat.CurrentStats != null)
                UpdateStats(_stat.CurrentResult);
        }
    }

    private void OnDisable()
    {
        if (_stat != null)
            _stat.OnStatsUpdated -= UpdateStats;
    }

    private void UpdateStats(StatResult result)
    {
        Stat stat = result.FinalStat;

        if (_currentHp > stat.MaxHp)
        {
            _currentHp = stat.MaxHp;
        }
        PublishHealthEvent();
    }

    public void TakeDamage(float damage)
    {
        if (_currentHp <= 0) return;

        float defense = _stat.CurrentStats.Defense;//
        float finalDamage = Mathf.Max(damage - defense, 1);

        _currentHp = Mathf.Clamp(_currentHp - finalDamage, 0, MaxHp);

        PublishHealthEvent();

        if (_currentHp <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (_currentHp <= 0) return;

        _currentHp = Mathf.Clamp(_currentHp + amount, 0, MaxHp);
        PublishHealthEvent();
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} 사망");
        OnDeath?.Invoke();
    }

    private void PublishHealthEvent()
    {
        // C# 이벤트 (기존 유지)
        OnHealthChanged?.Invoke(_currentHp, MaxHp);

        // ★ [수정] 구조체(HealthData)로 포장해서 발송
        if (_healthChannel != null)
        {
            _healthChannel.RaiseEvent(new HealthData(_currentHp, MaxHp));
        }
    }

}
