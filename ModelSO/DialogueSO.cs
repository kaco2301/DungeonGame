using System.Collections.Generic;
using UnityEngine;
using static Define;

[CreateAssetMenu(fileName = "New DialogueSO", menuName = "Dialogue/DialogueSO")]
public class DialogueSO : ScriptableObject
{
    public int startNodeID = 0;
    public List<DialogueNode> allNodes;
}

[System.Serializable]
public class DialogueNode
{
    public int nodeID; // 각 노드를 구분할 고유 ID (예: 1, 2, 3...)
    [TextArea] public string npcText; // 이 노드에서 NPC가 하는 말
    public List<Choice> choices; // 이 노드에서 플레이어가 할 수 있는 선택지 목록
}

[System.Serializable]
public class Choice
{
    public string choiceText; // 선택지 텍스트 (예: "퀘스트를 본다", "떠난다")
    public int nextNodeID;    // 이 선택지를 골랐을 때 연결될 다음 DialogueNode의 ID

    // 이 선택지에 퀘스트 수락/완료 같은 특별한 기능이 있는지 표시
    public QuestSO relatedQuest; // 이 선택지가 퀘스트를 제안/완료하는 경우

    // 【추가】 이 선택지가 특정 퀘스트 상태일 때만 보이도록 하는 조건
    public QuestSO requiredQuest;
    public QuestStatus requiredStatus;
}