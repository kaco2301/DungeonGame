using Kaco.UI.Inventory;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "ItemSO/Weapon")]
public class WeaponSO : EquippableItemSO
{
    public Define.WeaponType type;
    public GameObject weaponPrefab;

    [Header("Weapon Stats")]
    public int weaponAttackPower;

    [Header("Combo Info")]
    public int maxCombo; // 이 무기의 최대 콤보 횟수

    private void OnValidate()
    {
        SetEquipmentType(Define.EquipmentType.Weapon);
    }
}
