using Kaco.UI.Inventory;
using System.Collections.Generic;
using UnityEngine;
//Stat 수정기
[RequireComponent(typeof(Health))]
public class PlayerStat : EntityStat
{
    [SerializeField] private PlayerStatSO baseStatSO;
    [SerializeField] private EquipmentSO equipmentData;

    [SerializeField] private StatEventChannelSO _statChannel;
    [SerializeField] private ExpEventChannelSO _expChannel;
    [SerializeField] private FloatEventChannelSO _combatPowerChannel;

    public float CurrentCombatPower { get; private set; }
    public int Exp { get; set; }
    public int Gold { get; set; }

    protected override void InitializeStats()
    {
        // 1. 기본 스탯 설정 (SO -> BaseStat)
        // EntityStat의 protected _baseStats에 할당
        _baseStats = new Stat
        {
            Strength = baseStatSO.Strength,
            Dexterity = baseStatSO.Dexterity,
            Intelligence = baseStatSO.Intelligence,
            Vitality = baseStatSO.Vitality,
            Spirit = baseStatSO.Spirit,

            Level = baseStatSO.Level,
            PhysicalDmgBonus = baseStatSO.BasePhysicalDmgBonus,
            SpellDmgBonus = baseStatSO.BaseSpellDmg,
            MaxHp = baseStatSO.BaseMaxHp,
            MaxMana = baseStatSO.BaseMaxMana,
            Defense = baseStatSO.BaseDefense,
            MoveSpeed = baseStatSO.BaseMoveSpeed,
            AttackSpeed = baseStatSO.BaseAttackSpeed,
            ActionSpeed = baseStatSO.BaseActionSpeed
        };

        // 2. 기타 데이터 초기화
        Exp = baseStatSO.initialExp;
        Gold = baseStatSO.initialGold;

        // 3. 초기 장비 적용
        UpdateEquipmentStats(equipmentData.GetCurrentEquipmentState());
    }

    protected override void Start()
    {
        base.Start();
        BroadcastExp();
    }

    protected override void CalculateCustomStats(Stat stat)
    {
        stat.RunSpeed = stat.MoveSpeed * baseStatSO.RunMultiplier;
        stat.SprintSpeed = stat.MoveSpeed * baseStatSO.SprintMultiplier;
    }

    private void OnEnable()
    {
        equipmentData.OnEquipmentSlotChanged += OnEquipmentChangedHandler; 
        OnStatsUpdated += BroadcastStats;
    }

    private void OnDisable()
    {
        equipmentData.OnEquipmentSlotChanged -= OnEquipmentChangedHandler; 
        OnStatsUpdated -= BroadcastStats;
    }

    public void AddExp(int amount)
    {
        Exp += amount;

        int requiredExp = GetRequiredExp(_baseStats.Level);

        // 레벨업 처리
        while (Exp >= requiredExp)
        {
            Exp -= requiredExp;
            _baseStats.Level++;

            // 레벨 올랐으니 스탯 재계산 & 방송
            UpdateStats();
            requiredExp = GetRequiredExp(_baseStats.Level);
            _statChannel?.RaiseEvent(CurrentResult);
        }

        BroadcastExp();
    }

    private int GetRequiredExp(int level)
    {
        return level * 100;
    }

    private void BroadcastExp()
    {
        if (_expChannel != null)
        {
            int required = GetRequiredExp(_baseStats.Level);
            _expChannel.RaiseEvent(new ExpData(Exp, required));
        }
    }

    private void BroadcastStats(StatResult result)
    {
        _statChannel?.RaiseEvent(result);
    }

    private void OnEquipmentChangedHandler(Define.EquipmentType type, InventoryItem item)
    {
        UpdateEquipmentStats(equipmentData.GetCurrentEquipmentState());
    }

    private void UpdateEquipmentStats(IReadOnlyDictionary<Define.EquipmentType, InventoryItem> currentEquipment)
    {
        // 1. 기존 장비로 인한 Modifier 제거
        // (InventoryItem을 source로 가진 모든 modifier 삭제)
        _modifiers.RemoveAll(mod => mod.source is InventoryItem);

        // 2. 새 장비들의 Modifier 생성 및 추가
        foreach (var item in currentEquipment.Values)
        {
            if (item.item is EquippableItemSO equippableItem)
            {
                // EquippableItemSO에 정의된 modifierData들을 순회
                foreach (var modifierData in equippableItem.statModifiers)
                {
                    // 런타임 Modifier 생성 (source를 item으로 지정하여 추후 식별 가능)
                    StatModifier runtimeMod = modifierData.CreateRuntimeModifier(item);
                    if (runtimeMod != null)
                    {
                        _modifiers.Add(runtimeMod);
                    }
                }
            }
        }

        // 3. 스탯 재계산 (부모 클래스 메서드)
        UpdateStats();
        
        // 4. 전투력 등 파생 로직 처리
        RecalculateCombatPower(currentEquipment);
    }

    private void RecalculateCombatPower(IReadOnlyDictionary<Define.EquipmentType, InventoryItem> currentEquipment)
    {
        float currentWeaponAttack = 0f;
        if (currentEquipment.TryGetValue(Define.EquipmentType.Weapon, out InventoryItem weaponItem) &&
            weaponItem.item is WeaponSO weaponSO)
        {
            currentWeaponAttack = weaponSO.weaponAttackPower;
        }

        // CurrentStats는 이제 계산이 완료된 최신 상태입니다.
        CurrentCombatPower = currentWeaponAttack * (1f + CurrentStats.PhysicalDmgBonus);
        _combatPowerChannel?.RaiseEvent(CurrentCombatPower);
    }


}
