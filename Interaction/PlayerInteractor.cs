using Kaco.InputSystem;
using Kaco.UI.Inventory;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// 책임 1. 거리 계산
// 책임 2. 감지
// 책임 3. TryInteract 실행
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] public InventorySO inventoryData;
    [SerializeField] private InputReader inputReader; 

    private readonly HashSet<IInteractable> _nearby = new();
    private IInteractable _current = null;
    private IInteractable _nearest;

    [Header("Event Channels")]

    [SerializeField] private EventBusSO _interactionBus;

    private InteractionStartedEventChannel _startedEvent;
    private InteractableFoundEventChannel _foundEvent;
    private InteractionLostEventChannel _lostEvent;
    private InteractionEndedEventChannel _endedEvent;


    [SerializeField] private InteractionHoldingEventChannel holdProgressEvent;
    [SerializeField] private VoidEventChannelSO holdCanceledEvent;

    private bool _isHolding = false;
    private float _holdTimer = 0f;

    private bool _needsUpdate = false;

    private void Awake()
    {
        _startedEvent = _interactionBus.GetChannel<InteractionStartedEventChannel>();
        _foundEvent = _interactionBus.GetChannel<InteractableFoundEventChannel>();
        _lostEvent = _interactionBus.GetChannel<InteractionLostEventChannel>();
        _endedEvent = _interactionBus.GetChannel<InteractionEndedEventChannel>();
    }
    private void OnEnable()
    {
        inputReader.onInteractPerformed += HandleTap;
        inputReader.onInteractHoldStarted += HandleHold;
        inputReader.onInteractHoldCanceled += ResetHold;

        _endedEvent.Register(OnInteractionEnded);
    }
    private void OnDisable()
    {
        inputReader.onInteractPerformed -= HandleTap;
        inputReader.onInteractHoldStarted -= HandleHold;
        inputReader.onInteractHoldCanceled -= ResetHold;

        _endedEvent.Unregister(OnInteractionEnded);
    }

    private void Update()
    {
        if (_needsUpdate)
        {
            UpdateNearest();
            _needsUpdate = false; // 플래그 초기화

        }
        HandleHoldingTick();
    }


    #region ## Trigger Detection ##
    //범위 안에 들어온 오브젝트를 리스트에 추가하고 UI 갱신
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            if (_nearby.Add(interactable))
            {
                Debug.Log($"[Interactor] Added to nearby: {((MonoBehaviour)interactable).name}");
                interactable.OnRemoved += HandleInteractableRemoved; // [!!] OnRemoved 구독
                _needsUpdate = true;
            }
        }
    }

    //범위에서 나간 오브젝트를 감지하여 이벤트를 종료하고 리스트에서 제거 후 UI 갱신
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            // [!!] Remove가 true를 반환하면 (제거에 성공하면) 이벤트 구독 해지
            if (_nearby.Remove(interactable))
            {
                Debug.Log($"[Interactor] Removed from nearby: {((MonoBehaviour)interactable).name}");
                interactable.OnRemoved -= HandleInteractableRemoved; // [!!] OnRemoved 구독 해지
            }

            if (interactable == _current)
            {
                _endedEvent.RaiseEvent();
            }

            _needsUpdate = true;
        }
    }

    private void HandleInteractableRemoved(IInteractable removedItem)
    {
        if (_nearby.Remove(removedItem))
        {
            removedItem.OnRemoved -= HandleInteractableRemoved;
            _needsUpdate = true;
            Debug.Log($"[Interactor] '{((MonoBehaviour)removedItem).name}' was removed by event.");
        }
    }
    #endregion

    #region ## Closest Calculate ##
    public void UpdateNearest()
    {
        _nearby.RemoveWhere(item =>
            item == null ||
            (MonoBehaviour)item == null ||
            !item.IsAvailable() // [!!] IsAvailable() 사용으로 변경
        );

        // 1. 상태 확인 (Guard Clause): 현재 상호작용 중이면 프롬프트를 끈다
        if (_current != null)
        {
            if (_nearest != null)
            {
                _lostEvent.RaiseEvent();
            }
            _nearest = null;
            return;
        }

        // 2. 상태 저장: 이전에 가장 가까웠던 대상을 기억
        IInteractable previous = _nearest;

        // 3. 검색: 범위 내에서 새 대상을 검색
        _nearest = FindNearest();

        if (previous != _nearest)
        {
            if (_nearest != null)
                _foundEvent.RaiseEvent(_nearest.GetData());
            else
                _lostEvent.RaiseEvent();
        }
    }
    /// <summary>   
    /// 가장 가까운 대상을 찾아서 리턴
    /// </summary>
    private IInteractable FindNearest()
    {
        IInteractable nearest = null;
        float max = float.MaxValue;

        Vector3 pos = transform.position;
        // 거리 계산하여 가장 가까운 대상 찾기
        foreach (var obj in _nearby)
        {
            if (!obj.IsAvailable())
            {
                continue; // 상호작용 불가능하면(콜라이더가 꺼졌거나 수량이 0이면) 건너뜀
            }

            Vector3 targetPosition = ((MonoBehaviour)obj).transform.position;
            float dist = Vector3.Distance(transform.position, targetPosition);

            if (dist < max)
            {
                max = dist;
                nearest = obj;
            }
        }
        return nearest;
    }
    #endregion ##


    #region ## Interaction Execution ##
    /// <summary>   
    /// 상호작용 키를 눌렀을 때, 저장된 가장 가까운 대상을 사용합니다.
    /// </summary>
    private void TryInteract()
    {
        if (_current != null || _nearest == null) return;

        IInteractable target = _nearest;

        if (target.Interact(this))
        {
            StartInteraction(target);
        }

        _needsUpdate = true;
    }

    private void StartInteraction(IInteractable target)
    {
        _current = target;
        _nearest = null;
        _needsUpdate = true;
        _startedEvent.RaiseEvent(target.GetData());
    }


    private void OnInteractionEnded()
    {
        if (_current == null) return;

        _current = null;
        _needsUpdate = true;
    }

    #endregion ##

    #region ## Tap and Hold ##
    private void HandleTap()
    {
        if (_nearest == null || _current != null) return;

        if (_nearest.GetData().InteractType == Define.InteractionType.Tap)
        {
            TryInteract();
        }
    }

    private void HandleHold()
    {
        if (_nearest == null || _current != null) return;

        if (_nearest.GetData().InteractType == Define.InteractionType.Hold)
        {
            _isHolding = true;
            _holdTimer = 0f;
        }
    }

    private void ResetHold()
    {
        if (!_isHolding) return;

        _isHolding = false;
        _holdTimer = 0f;
        holdCanceledEvent.RaiseEvent();

    }

    private void HandleHoldingTick()
    {
        if (!_isHolding) return;

        float requiredTime = _nearest.HoldDuration;
        if (requiredTime <= 0) requiredTime = 0.01f;

        _holdTimer += Time.deltaTime;
        float fill = Mathf.Clamp01(_holdTimer / requiredTime);

        holdProgressEvent.RaiseEvent(fill);

        if (fill >= 1f)
        {
            _isHolding = false;
            _holdTimer = 0f;
            TryInteract();
        }
    }
    #endregion ##

}


