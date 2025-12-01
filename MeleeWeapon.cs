using System.Collections.Generic;
using UnityEngine;

public interface IMeleeWeapon
{
    void Attack();
    string GetAttackAnimationName();
}

public class MeleeWeapon : WeaponBase
{
    [SerializeField] private LayerMask enemyLayer;

    [Header("Attack Points")]
    [SerializeField] private Transform weaponBasePoint;
    [SerializeField] private Transform weaponTipPoint;

    //[SerializeField] private float defaultHitWindowDuration = 0.2f;
    private HashSet<Collider> _hitTargets = new HashSet<Collider>();
    private Vector3 _previousCenterPosition; // 이전 프레임의 '중심점' 위치를 저장
    private bool _isAttacking = false;

    private MeleeWeaponSO MeleeData => weaponData as MeleeWeaponSO;


    public override void Attack()
    {
        throw new System.NotImplementedException();
    }

    public override void EnableHit()
    {
        Debug.Log("<color=green>DAMAGE WINDOW OPENED (EnableHit called)</color>");
        if (MeleeData == null)
        {
            Debug.Log("null이당"); return;
        }

        _isAttacking = true;
        _hitTargets.Clear();

        _previousCenterPosition = Vector3.Lerp(weaponBasePoint.position, weaponTipPoint.position, 0.5f);// SphereCast의 시작점으로 현재 위치 기록
    }

    public override void DisableHit()
    {
        Debug.Log("<color=red>DAMAGE WINDOW CLOSED (DisableHit called)</color>");
        Debug.Log("aaakk");
        _isAttacking = false;
    }

    public void OnAttackEnd()
    {
        _isAttacking = false;
    }

    /// <summary>
    /// 물리 프레임마다 호출되며, 실제 공격 판정을 수행합니다.
    /// </summary>
    private void FixedUpdate()
    {
        if (!_isAttacking) return;
        if (weaponBasePoint == null || weaponTipPoint == null) return;

        float radius = Vector3.Distance(weaponBasePoint.position, weaponTipPoint.position) / 2f;
        Vector3 currentCenter = Vector3.Lerp(weaponBasePoint.position, weaponTipPoint.position, 0.5f);

        Vector3 direction = currentCenter - _previousCenterPosition;
        float distance = direction.magnitude;

        if (distance > 0)
        {
            // SphereCastAll을 사용하여 이전 위치에서 현재 위치까지의 경로상 모든 충돌체를 감지
            RaycastHit[] hits = Physics.SphereCastAll(
                _previousCenterPosition,      // 시작점
                radius, // 판정 두께
                direction.normalized,   // 방향
                distance,
                enemyLayer// 거리
            );

            foreach (var hit in hits)
            {
                Debug.Log($"SphereCast가 {hit.collider.name}에 부딪힘! (레이어: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");

                Collider target = hit.collider;

                // 이미 이 공격에서 맞은 타겟이거나, 자기 자신이라면 무시
                if (_hitTargets.Contains(target) || target.transform.root == this.transform.root)
                    continue;

                // IDamageable 인터페이스를 가진 컴포넌트를 찾아 TakeDamage 호출
                if (target.TryGetComponent<IDamageable>(out var damageable))
                {
                    //float playerAttackPower = Managers.Game.PlayerStats.CurrentStats.PhysicalDmgBonus;
                    //damageable.TakeDamage((int)playerAttackPower);
                    Instantiate(MeleeData.hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    _hitTargets.Add(target); // 중복 피격 방지를 위해 리스트에 추가
                }
            }
        }
        // 다음 프레임을 위해 현재 위치를 이전 위치로 저장
        _previousCenterPosition = currentCenter;
    }
    

    /// <summary>
    /// 씬 뷰에서 공격 판정 범위를 시각적으로 보여줍니다.
    /// </summary>
    private void OnDrawGizmos()
    {
        // 기즈모도 동적으로 계산된 반지름과 중심점을 사용하도록 변경
        if (weaponBasePoint != null && weaponTipPoint != null)
        {
            float radius = Vector3.Distance(weaponBasePoint.position, weaponTipPoint.position) / 2f;
            Vector3 center = Vector3.Lerp(weaponBasePoint.position, weaponTipPoint.position, 0.5f);

            Gizmos.color = _isAttacking ? Color.red : Color.green;
            Gizmos.DrawWireSphere(center, radius);
        }
    }
}
