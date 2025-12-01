using Kaco.UI.Inventory;
using System.Collections.Generic;
using UnityEngine;

public class QuestSO : ScriptableObject
{
    [Header("Information")]
    public int questID;//퀘스트 아이디
    public string questTitle;//퀘스트 제목
    [TextArea] public string description;

    [Header("Targets")]
    public List<QuestObjective> objectives;

    [Header("Rewards")]
    public QuestReward rewards;

    [Header("State")]
    public bool isCompleted = false;
}

[System.Serializable]
public class QuestObjective
{
    public Define.ObjectiveType type;

    public string targetName; // 몬스터 이름, 아이템 이름, NPC 이름
    public int amount;
    [HideInInspector] public int currentAmount = 0;
}

[System.Serializable]
public struct QuestReward
{
    public int exp;
    public int gold;
    public List<ItemSO> item;
}