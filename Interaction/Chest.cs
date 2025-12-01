using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 월드에 배치되어 상호작용 가능한 '상자' 오브젝트입니다.
/// IInteractable을 구현하며, 고유한 InventorySO의 복사본(_runtimeStash)을 소유합니다.
/// 상호작용 시, UIManager를 통해 StoragePanel을 열고
/// 패널에 상자 데이터와 InventoryMediator를 주입하는 책임을 가집니다.
/// </summary>
namespace Kaco.UI.Inventory
{
    [RequireComponent(typeof(AudioSource), typeof(Animation))]
    public class Chest : MonoBehaviour, IInteractable
    {
        [Header("Data Template")]
        [SerializeField] private InventorySO stashTemplateSO;
        private InventorySO _runtimeStash;

        [Header("Loot Settings")]
        [SerializeField] private LootPoolSO _lootPool; // 랜덤 아이템을 뽑아줄 풀
        [SerializeField] private List<CategoryModifier> _categoryModifiers;

        [SerializeField] private InventoryMediator _mediator;

        private bool _isLootGenerated = false;
        public float HoldDuration => 1.5f;

        public event Action<IInteractable> OnRemoved;

        private void Start()
        {
            _runtimeStash = Instantiate(stashTemplateSO);
            _runtimeStash.Initialize();
        }

        public InteractableData GetData()
        {
            return new InteractableData
            {
                InteractType = Define.InteractionType.Hold,
                UIType = Define.InteractionUIType.Prompt,
                InteractionText = "To Open"
            };
        }

        public bool Interact(PlayerInteractor interactor)
        {
            if (!_isLootGenerated)
            {
                GenerateLoot();
                _isLootGenerated = true;
            }
            if (_mediator == null)
            {
                Debug.LogError("InteractionController 참조가 없습니다.");
                return false;
            }

            var storagePanel = UIManager.Instance.ShowChestPanel();

            if (storagePanel != null)
            {
                // 3. 패널에 상자 데이터와 Mediator 주입
                storagePanel.Initialize(_runtimeStash, _mediator);
                Debug.Log("Chest Panel Initialized");
                UIManager.Instance.ToggleCharacterUI();
                return true; // 상호작용이 '시작'되었음을 알림
            }
            else
            {
                Debug.LogError("StoragePanel을 여는 데 실패했습니다.");
                return false;
            }
        }

        /// <summary>
        /// LootPool을 돌려서 인벤토리에 아이템을 AddItem 합니다.
        /// </summary>
        private void GenerateLoot()
        {
            if (_lootPool == null) return;

            // 1. 보정치 딕셔너리 변환
            Dictionary<LootCategory, float> modifiers = new Dictionary<LootCategory, float>();
            foreach (var mod in _categoryModifiers)
            {
                if (!modifiers.ContainsKey(mod.Category))
                    modifiers.Add(mod.Category, mod.Multiplier);
            }

            // 2. 아이템 뽑기
            List<ItemSO> lootedItems = _lootPool.RollLoot(modifiers);

            // 3. [질문하신 부분] 뽑은 아이템을 InventorySO에 집어넣기
            foreach (var item in lootedItems)
            {
                // 수량은 일단 1개로 가정 (필요시 LootEntry에 Min/Max Quantity 추가 가능)
                int leftOver = _runtimeStash.AddItem(item, 1);

                if (leftOver > 0)
                {
                    Debug.Log("상자가 꽉 차서 일부 아이템이 들어가지 못했습니다.");
                }
            }
        }

        public bool IsAvailable()
        {
            return true;
        }
    }
}
