using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    protected WeaponSO weaponData;

    public WeaponSO WeaponData => weaponData;

    public void Init(WeaponSO data)
    {
        this.weaponData = data;
    }

    public abstract void Attack();
    public virtual void EnableHit() { }
    public virtual void DisableHit() { }
}
