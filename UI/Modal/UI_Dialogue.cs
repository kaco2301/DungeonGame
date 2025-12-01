using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//주의사항 dialogueData는 null 데이터가 있어서는 안됨.
public struct DialogueData : IPanelData
{
    public string NpcName;
    public string NpcText;
    public List<Choice> Choices;
    public IDialogueController Controller;

    public DialogueData(string npcName, string npcText, List<Choice> choices, IDialogueController controller)
    {
        NpcName = npcName;
        NpcText = npcText;
        Choices = choices;
        Controller = controller;
    }
}

public class UI_Dialogue : UI_Modal, IDataPanel<DialogueData>
{
    protected enum Texts
    {
        NpcName,
        NpcDialogue
    }

    protected enum Buttons
    {
        Answer_00,
        Answer_01,
        Answer_02,
        Answer_Goodbye
    }

    private TMP_Text NpcName => Get<TMP_Text>((int)Texts.NpcName);
    private TMP_Text NpcDialogue => Get<TMP_Text>((int)Texts.NpcDialogue);

    private Button Answer_00 => Get<Button>((int)Buttons.Answer_00);
    private Button Answer_01 => Get<Button>((int)Buttons.Answer_01);
    private Button Answer_02 => Get<Button>((int)Buttons.Answer_02);
    private Button Answer_Goodbye => Get<Button>((int)Buttons.Answer_Goodbye);

    private List<ChoiceButton> _choiceButtons = new List<ChoiceButton>();

    public override void Init()
    {
        base.Init();
        Bind<TMP_Text>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        _choiceButtons.Add(Answer_00.GetComponent<ChoiceButton>());
        _choiceButtons.Add(Answer_01.GetComponent<ChoiceButton>());
        _choiceButtons.Add(Answer_02.GetComponent<ChoiceButton>());
        _choiceButtons.Add(Answer_Goodbye.GetComponent<ChoiceButton>());
    }
    /// <summary>
    /// NPC 등 다른 클래스에서 호출할 메서드입니다.
    /// </summary>
    /// <param name="choices">표시할 대화 노드 리스트</param>
    /// <param name="npc">어떤 NPC가 이 대화를 요청했는지에 대한 정보</param>
    public void SetData(DialogueData data)
    {
        NpcName.text = data.NpcName;
        NpcDialogue.text = data.NpcText;

        foreach (ChoiceButton button in _choiceButtons)
        {
            button.ResetData();
        }

        // 2. 전달받은 선택지 목록만큼 버튼을 동적으로 생성
        for (int i = 0; i < data.Choices.Count; i++)
        {
            // 리스트에 있는 버튼 개수를 초과하지 않도록 방어
            if (i < _choiceButtons.Count)
            {
                _choiceButtons[i].SetData(data.Choices[i], data.Controller);
            }
        }
    }

    public override void Hide()
    {
        base.Hide();

        foreach (ChoiceButton button in _choiceButtons)
        {
            button.ResetData();
        }
    }


}
