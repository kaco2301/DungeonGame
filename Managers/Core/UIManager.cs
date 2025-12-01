using Kaco.InputSystem;
using Kaco.UI.Inventory;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using static UnityEngine.UI.InputField;

public interface IPanelData { }

public interface IDataPanel<T> where T : IPanelData
{
    void SetData(T data);
}

[System.Serializable]
public struct InteractionPanelEntry
{
    public InteractionUIType type;
    public UI_Interaction panel;
}
//1. Found / Lost UI 처리

//Tap/Hold UI 처리(프로그레스 바)

//Interaction 패널 표시/숨김
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Method Managers")]
    private InteractionManager _interaction;
    private PopupManager _popup;
    private ModalManager _modal;

    [SerializeField] private List<InteractionPanelEntry> interactionPanels;

    [Header("Core Systems")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private MouseFollower _mouseFollower;

    [Header("Managed Scene UI")]
    [SerializeField] private UI_Character _characterUI;

    [Header("Modal Panels")]
    [SerializeField] private UI_Dialogue _dialogueUI;
    [SerializeField] private DialogueController _dialogueController;

    [Header("Popup Panels")]
    [SerializeField] private UI_Chest _chest;

    [Header("Interaction Panels")]

    [Header("Event Channels (Input from PlayerInteractor)")]
    [SerializeField] private EventBusSO _eventBus;

    private InteractionStartedEventChannel _startedEvent;
    private InteractableFoundEventChannel _foundEvent;
    private InteractionLostEventChannel _lostEvent;
    private InteractionEndedEventChannel _endedEvent;

    [SerializeField] private InteractionHoldingEventChannel holdProgressEvent;
    [SerializeField] private InteractionHoldCanceledEventChannel holdCanceledEvent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _modal = new ModalManager();
        _popup = new PopupManager();
        _interaction = new InteractionManager(interactionPanels);

        _startedEvent = _eventBus.GetChannel<InteractionStartedEventChannel>();
        _foundEvent = _eventBus.GetChannel<InteractableFoundEventChannel>();
        _lostEvent = _eventBus.GetChannel<InteractionLostEventChannel>();
        _endedEvent = _eventBus.GetChannel<InteractionEndedEventChannel>();
    }

    private void OnEnable()
    {
        if (inputReader != null)
        {
            inputReader.onInventoryPerformed += ToggleCharacterUI;
            inputReader.onEscPerformed += HandleEscPressed;
        } // 1번 항목에서 수정된 핸들러

        if (_dialogueController != null)
        {
            _dialogueController.OnShowDialogueNode += HandleShowDialogue;
            _dialogueController.OnHideDialogue += HandleHideDialogue;
        }

        if (_foundEvent != null) _foundEvent.Register(_interaction.Show);
        if (_lostEvent != null) _lostEvent.Register(_interaction.Hide);
        if (holdProgressEvent != null) holdProgressEvent.Register(_interaction.UpdateFill);
        if (holdCanceledEvent != null) holdCanceledEvent.Register(_interaction.CancelFill);
        if (_startedEvent != null) _startedEvent.Register(OnInteractionStarted); 
        if (_endedEvent != null) _endedEvent.Register(HandleInteractionEnded);
    }

    private void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.onInventoryPerformed -= ToggleCharacterUI;
            inputReader.onEscPerformed -= HandleEscPressed;
        }
        if (_dialogueController != null)
        {
            _dialogueController.OnShowDialogueNode -= HandleShowDialogue;
            _dialogueController.OnHideDialogue -= HandleHideDialogue;
        }

        if (_foundEvent != null) _foundEvent.Unregister(_interaction.Show);
        if (_lostEvent != null) _lostEvent.Unregister(_interaction.Hide);
        if (holdProgressEvent != null) holdProgressEvent.Unregister(_interaction.UpdateFill);
        if (holdCanceledEvent != null) holdCanceledEvent.Unregister(_interaction.CancelFill);
        if (_startedEvent != null) _startedEvent.Unregister(OnInteractionStarted); 
        if (_endedEvent != null) _endedEvent.Unregister(HandleInteractionEnded);
    }



    private void Start()
    {
        _mouseFollower.Toggle(false);
        _characterUI.Hide();
        _dialogueUI.Hide();
        _chest.Hide();
        _interaction.HideAll();
    }

    public void ShowMouseFollower(InventoryItem item)
    {
        if (item.IsEmpty)
            return;

        _mouseFollower.SetData(item);
        _mouseFollower.Toggle(true);
    }

    public void HideMouseFollower() => _mouseFollower.Toggle(false);

    private void HandleEscPressed()
    {
        if (_popup.Current == null) return;

        // [!!] 닫기 전에 닫히는 팝업이 무엇인지 받아옵니다. [!!]
        UI_Popup closedPopup = _popup.CloseCurrentPopup(); //

        // [!!] 닫힌 팝업이 상호작용 팝업(_chest)인지 확인합니다. [!!]
        if (closedPopup != null && closedPopup.IsInteractionPopup) //
        {
            // [!!] PlayerInteractor에게 상호작용이 끝났다고 알립니다. [!!]
            if (_endedEvent != null)
            {
                _endedEvent.RaiseEvent(); //
            }
            else
            {
                Debug.LogWarning("UIManager에 endedEvent가 할당되지 않았습니다.");
            }
        }
    }

    private void HandleInteractionEnded()
    {
        // 1. 상호작용 프롬프트 숨김
        _interaction.Hide();

        // 2. 열려있는 팝업(상자 등) 모두 닫기
        _popup.CloseAllPopup();
    }

    private void OnInteractionStarted(InteractableData data)
    {
        _interaction.Hide();
    }

    private void HandleShowDialogue(DialogueData data)
    {
        _modal.ShowModal<DialogueData>(_dialogueUI, data);
    }

    private void HandleHideDialogue()
    {
        _modal.CloseCurrentModalPanel();
        _endedEvent.RaiseEvent(); //
    }

    public UI_Chest ShowChestPanel()
    {
        return _popup.ShowPopup(_chest) as UI_Chest;
    }

    public void ToggleCharacterUI()
    {
        _characterUI.Toggle();
    }
}