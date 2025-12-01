using System;
using UnityEngine;

public interface IDialogueController
{
    event Action OnDialogueStarted;
    event Action OnDialogueEnded;

    void StartDialogue(DialogueSO dialogue, IDialogueOwner owner);
    void SelectChoice(Choice choice);
    void EndDialogue();
}

public class DialogueController : MonoBehaviour, IDialogueController
{
    public static DialogueController Instance { get; private set; }

    private IDialogueOwner _currentOwner;
    private DialogueSO _currentDialogue;

    public event Action OnDialogueStarted;
    public event Action OnDialogueEnded;
    public event Action<DialogueData> OnShowDialogueNode;
    public event Action OnHideDialogue;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void ShowNode(DialogueNode node)
    {
        var data = new DialogueData(_currentOwner.GetDialogueName(), node.npcText, node.choices, this);
        OnShowDialogueNode?.Invoke(data);
    }

    public void StartDialogue(DialogueSO dialogueData, IDialogueOwner owner)
    {
        _currentOwner = owner;
        _currentDialogue = dialogueData;

        var start = FindNodeByID(dialogueData.startNodeID);
        if (start == null || start.choices.Count == 0) { EndDialogue(); return; }

        ShowNode(start);
    }

    public void EndDialogue()
    {
        _currentOwner = null;
        _currentDialogue = null;
        OnHideDialogue?.Invoke();
    }

    public void SelectChoice(Choice choice)
    {
        var next = FindNodeByID(choice.nextNodeID);

        if (next == null || next.choices.Count == 0) { EndDialogue(); return; }
        ShowNode(next);
    }

    private DialogueNode FindNodeByID(int id)
    {
        foreach (var node in _currentDialogue.allNodes)
        {
            if (node.nodeID == id)
                return node;
        }
        return null;
    }
}
