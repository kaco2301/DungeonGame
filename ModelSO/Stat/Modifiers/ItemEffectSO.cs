using UnityEngine;

namespace Kaco.UI.Inventory
{
    public abstract class ItemEffectSO : ScriptableObject
    {
        [Header("UI Display Info")]
        public Sprite Icon; // (예: 하트 아이콘)
        public string EffectName; // (예: "HP")

        [TextArea] 
        public string Description;

        /// <summary>
        /// 아이템 사용 시 효과를 실행합니다.
        /// </summary>
        public abstract void ExecuteEffect(GameObject target, float value);
    }
}
    