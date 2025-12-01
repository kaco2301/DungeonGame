using UnityEngine;

namespace Kaco.UI.Inventory
{
    [CreateAssetMenu(menuName = "Item Effects/Mana Effect")]
    public class ManaEffectSO : ItemEffectSO
    {
        public override void ExecuteEffect(GameObject target, float value)
        {
            var mana = target.GetComponent<Mana>();
            if (mana != null)
            {
                mana.RestoreMana(value);
            }
        }
    }
}
