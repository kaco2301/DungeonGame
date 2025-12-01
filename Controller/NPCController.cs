using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public interface IDialogueOwner
{
    string GetDialogueName();
    Transform GetCameraTarget();
    Transform GetLookAtTarget();
}

public class NPCController : MonoBehaviour, IInteractable, IDialogueOwner
{
    [Header("Dialogue Camera Targets")]
    [SerializeField] private Transform cameraPositionTarget; // 카메라가 위치할 지점
    [SerializeField] private Transform lookAtTarget; // NPC의 머리 근처

    [Header("Data")]
    [SerializeField] private DialogueSO dialogue;
    [SerializeField] private List<QuestSO> availableQuests;
    [SerializeField] public string npcName;

    private IDialogueController _dialogueController;

    public float HoldDuration => 0F;

    public event Action<IInteractable> OnRemoved;

    private void Start()
    {
        _dialogueController = DialogueController.Instance;
    }

    public InteractableData GetData()
    {
        return new InteractableData
        {
            InteractType = InteractionType.Tap,
            UIType = InteractionUIType.Prompt,
            InteractionText = "To Talk"
        };
    }

    public bool Interact(PlayerInteractor interactor)
    {
        if(_dialogueController == null)
        {
            Debug.Log("kkkk");return false;
        }
        _dialogueController.StartDialogue(dialogue, this);
        return true;
    }

    public string GetDialogueName()
    {
        return npcName;
    }

    public Transform GetCameraTarget()
    {
        return cameraPositionTarget;
    }

    public Transform GetLookAtTarget()
    {
        return lookAtTarget;
    }

    public bool IsAvailable()
    {
        return true;
    }
}
