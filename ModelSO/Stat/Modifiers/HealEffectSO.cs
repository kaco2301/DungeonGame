using UnityEngine;

namespace Kaco.UI.Inventory
{
    [CreateAssetMenu(fileName = "HealEffect", menuName = "Stats/ItemEffect/Heal Effect")]
    public class HealEffectSO : ItemEffectSO
    {
        public override void ExecuteEffect(GameObject target, float value)
        {
            // 대상에게서 Health 컴포넌트 찾기
            var health = target.GetComponent<Health>();

            if (health != null)
            {
                health.Heal(value);
                // 만약 퍼센트 힐이라면: health.Heal(health.MaxHp * value); 로직 추가 가능
            }
        }
    }
}