using UnityEngine;

namespace Kaco.UI.Inventory
{
    [CreateAssetMenu]
    public class ItemParameterSO : ScriptableObject
    {
        [field: SerializeField]
        public string ParameterName { get; private set; }

    }
}