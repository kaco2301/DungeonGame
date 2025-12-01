using Kaco.UI.Inventory;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [Header("Player Data (Owned by this Controller)")]
    [SerializeField] private InventorySO inventoryData;
    [SerializeField] private EquipmentSO equipmentData;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private UI_Character _character;
    [SerializeField] private InventoryMediator _mediator; 
    
    private IInventoryService _inventoryService;

    public List<InventoryItem> initializeItems = new List<InventoryItem>();

    private void Awake()
    {
        _inventoryService = new InventoryService();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        inventoryData.Initialize();
        equipmentData.Initialize();

        _mediator.Initialize(_inventoryService);

        if (_character != null)
        {
            _character.Initialize(inventoryData, equipmentData, _mediator);
        }

        foreach (InventoryItem item in initializeItems)
        {
            if (item.IsEmpty) continue;
            inventoryData.AddItem(item);
        }
    }


    public InventorySO GetInventoryData()
    {
        return inventoryData;
    }

    public EquipmentSO GetEquipmentData()
    {
        return equipmentData;
    }
}
