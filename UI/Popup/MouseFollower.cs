using Kaco.InputSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaco.UI.Inventory
{
    public class MouseFollower : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image itemImage;
        [SerializeField] private TMP_Text quantityText;

        [Header("System References")]
        [SerializeField] private InputReader inputReader;

        private Canvas _canvas;

        public void Awake()
        {
            _canvas = transform.root.GetComponent<Canvas>();
        }

        public void SetData(InventoryItem item)
        {
            itemImage.sprite = item.item.Icon;
            quantityText.text = item.quantity > 1 ? item.quantity.ToString() : "";

            Debug.Log("setdata");
        }

        private void Update()
        {
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_canvas.transform,
                inputReader.MousePosition,
                _canvas.worldCamera,
                out position);

            transform.position = _canvas.transform.TransformPoint(position);
        }

        public void Toggle(bool on)
        {
            gameObject.SetActive(on);
        }
    }
}