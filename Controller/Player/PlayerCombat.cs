using Kaco.InputSystem;
using Kaco.Player;
using Kaco.UI.Inventory;
using System;
using UnityEngine;

[RequireComponent(typeof(InputReader))]
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Transform _weaponHolster; // 무기 장착 위치
    [SerializeField] private EquipmentSO _equipmentSO; // 장비 데이터

    private PlayerAnimationController _animationController;
    private PlayerController _playerController;

    private WeaponBase _equippedWeapon;
    public WeaponBase CurrentWeapon => _equippedWeapon;

    // 콤보 상태
    private int _comboCounter = 0;
    private bool _isAttacking = false;
    private bool _nextAttackBuffered = false;
    private bool _comboWindowOpen = false;

    private void Awake()
    {
        _animationController = GetComponent<PlayerAnimationController>();
        _playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        _equipmentSO.OnEquipmentSlotChanged += UpdateWeaponVisual;
    }

    private void OnDisable()
    {
        _equipmentSO.OnEquipmentSlotChanged -= UpdateWeaponVisual;
    }

    private void UpdateWeaponVisual(Define.EquipmentType type, InventoryItem item)
    {
        if (type != Define.EquipmentType.Weapon) return;

        // 1. 기존 무기 제거
        if (_equippedWeapon != null)
        {
            Destroy(_equippedWeapon.gameObject);
            _equippedWeapon = null;
        }

        // 2. 무기 해제 상태거나 프리팹이 없으면 종료
        if (item.IsEmpty || !(item.item is WeaponSO weaponData) || weaponData.weaponPrefab == null)
        {
            _animationController.SetWeaponType(0);
            return;
        }

        // 3. 새 무기 생성 및 초기화
        GameObject weaponObj = Instantiate(weaponData.weaponPrefab, _weaponHolster);
        _equippedWeapon = weaponObj.GetComponent<WeaponBase>();

        if (_equippedWeapon != null)
        {
            int weaponTypeInt = (int)weaponData.type;
            _animationController.SetWeaponType(weaponTypeInt);
            _equippedWeapon.Init(weaponData);
        }

    }

    public void OnAttackInput()
    {
        // 공격 입력 처리 (같은 로직을 PlayerController에서 쓰던 것과 동일)
        if (_equippedWeapon == null)
        {
            // 빈손일 때 동작(예: 펀치 등) 원하면 여기 처리
            return;
        }

        if (!_isAttacking)
        {
            _isAttacking = true;
            _comboCounter = 0;
            PlayComboAttack(_comboCounter);
        }
        else if (_comboWindowOpen && _equippedWeapon != null && _comboCounter < _equippedWeapon.WeaponData.maxCombo)
        {
            _nextAttackBuffered = true;
        }
    }

    private int GetWeaponMaxCombo()
    {
        return _equippedWeapon?.WeaponData != null ? _equippedWeapon.WeaponData.maxCombo : 1;
    }

    private void PlayComboAttack(int comboIndex)
    {
        _nextAttackBuffered = false;

        if (_equippedWeapon == null || _animationController == null)
        {
            ResetCombo();
            return;
        }

        // animation controller가 애니메이션 트리거를 담당함
        _animationController.PlayAttackAnimation(comboIndex);

        // (선택) 만약 무기 자체의 Attack()가 필요하면 여기서 호출
        // _equippedWeapon.Attack();
    }

    private void ResetCombo()
    {
        _comboCounter = 0;
        _isAttacking = false;
        _nextAttackBuffered = false;
        _comboWindowOpen = false;
        if (_animationController != null)
            _animationController.SetComboIndex(0);
    }



    /// <summary>
    /// 애니메이션 이벤트: 공격 애니메이션 전체 종료
    /// </summary>
    public void OnAttackEnd()
    {
        // 애니메이션이 완전히 끝났을 때 콤보 리셋
        ResetCombo();
        // 무기 상태 초기화(안전)
        //_equippedWeapon?.OnAttackEnd();
    }

    /// <summary>
    /// 애니메이션 이벤트: 히트 윈도우 시작
    /// </summary>
    public void EnableHit()
    {
        _comboWindowOpen = true;
        // 무기에게 히트 시작을 전달 (무기 쪽에서 실제 판정 수행)
        _equippedWeapon?.EnableHit();
    }

    /// <summary>
    /// 애니메이션 이벤트: 히트 윈도우 종료
    /// </summary>
    public void DisableHit()
    {
        // 무기에게 히트 종료를 전달
        _equippedWeapon?.DisableHit();

        // 콤보 버퍼 처리: 예약된 다음 공격이 있으면 실행, 아니면 콤보 종료
        if (_nextAttackBuffered && _equippedWeapon != null && _comboCounter < GetWeaponMaxCombo() - 1)
        {
            _comboCounter++;
            PlayComboAttack(_comboCounter);
        }
        else
        {
            ResetCombo();
        }

        _comboWindowOpen = false;
    }
}

