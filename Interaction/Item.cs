using Kaco.UI.Inventory;
using System;
using System.Collections;
using UnityEngine;
using static Define;

[RequireComponent(typeof(AudioSource))]
public class Item : MonoBehaviour, IInteractable
{
    [field: SerializeField]
    public ItemSO InventoryItem { get; private set; }

    [field: SerializeField]
    public int Quantity { get; set; } = 1;

    public float HoldDuration => 0f;

    public event Action<IInteractable> OnRemoved;

    private Collider _col;
    private AudioSource _audioSource;

    [SerializeField]
    private float duration = 0.3f;

    private void Awake()
    {
        _col = GetComponent<Collider>();
        _audioSource = GetComponent<AudioSource>();
    }

    public InteractableData GetData()
    {
        return new InteractableData
        {
            InteractType = InteractionType.Tap,
            UIType = InteractionUIType.Indicator,
            InteractionText = "To Pick Up",
            Icon = InventoryItem.Icon,
            ItemName = InventoryItem.Name
        };
    }

    public bool IsAvailable()
    {
        return _col.enabled && Quantity > 0;
    }

    public bool Interact(PlayerInteractor interactor) // 수정 후: bool 반환
    {
        int leftOver = interactor.inventoryData.AddItem(InventoryItem, Quantity);
        if (leftOver == 0)
        {
            Quantity = 0;
            _col.enabled = false;
            StartCoroutine(AnimateItemPickUp());


            OnRemoved?.Invoke(this);
            return false;
        }
        else
        {
            this.Quantity = leftOver; // 일부만 획득했거나 못했으므로 '실패' 반환

            return false;
        }
    }

    private IEnumerator AnimateItemPickUp()
    {
        AudioManager.Instance.PlaySFX(_audioSource, "PickUp");
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, currentTime / duration);
            yield return null;
        }
        Destroy(gameObject);
    }
}


