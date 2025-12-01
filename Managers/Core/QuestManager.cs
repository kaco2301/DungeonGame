using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    private Dictionary<QuestSO, QuestProgress> _activeQuests = new Dictionary<QuestSO, QuestProgress>();
    // 완전히 보상까지 받은 퀘스트 ID 목록
    private HashSet<int> _turnedInQuests = new HashSet<int>();
    private void Awake()
    {
        Instance = this;
    }
    /// <summary>
    /// 특정 퀘스트의 현재 상태를 반환합니다.
    /// </summary>
    public Define.QuestStatus GetQuestStatus(QuestSO quest)
    {
        if (quest == null) return Define.QuestStatus.NotStarted;
        
        if (_turnedInQuests.Contains(quest.questID)) return Define.QuestStatus.TurnedIn;
        if (_activeQuests.ContainsKey(quest))
        {
            return _activeQuests[quest].IsComplete ? Define.QuestStatus.Completed : Define.QuestStatus.InProgress;
        }
        return Define.QuestStatus.NotStarted;
    }

    /// <summary>
    /// 새로운 퀘스트를 수락합니다.
    /// </summary>
    public void AcceptQuest(QuestSO quest)
    {
        if (GetQuestStatus(quest) != Define.QuestStatus.NotStarted) return;

        Debug.Log($"퀘스트 수락: {quest.questTitle}");
        _activeQuests.Add(quest, new QuestProgress(quest));
        // OnQuestStateChanged?.Invoke();
    }

    /// <summary>
    /// 퀘스트 진행도를 업데이트합니다. (예: 몬스터 사냥 시 호출)
    /// </summary>
    public void UpdateQuestProgress(string targetName, int amount)
    {
        foreach (var questProgress in _activeQuests.Values)
        {
            questProgress.UpdateProgress(targetName, amount);
        }
        // OnQuestStateChanged?.Invoke();
    }
    
    // TODO: 퀘스트 보상 지급 및 완료 처리 메서드
    public void TurnInQuest(QuestSO quest) {
    }
}
/// <summary>
/// 퀘스트의 실시간 진행 상황을 저장하는 클래스입니다. (SO 원본을 오염시키지 않기 위함)
/// </summary>
public class QuestProgress
{
    public QuestSO QuestData { get; }
    private Dictionary<string, int> _objectiveProgress; // 목표 이름, 현재 달성량
    public bool IsComplete => CheckCompletion();

    public QuestProgress(QuestSO questData)
    {
        QuestData = questData;
        _objectiveProgress = new Dictionary<string, int>();
        foreach (var objective in QuestData.objectives)
        {
            _objectiveProgress.Add(objective.targetName, 0);
        }
    }

    public void UpdateProgress(string targetName, int amount)
    {
        if (_objectiveProgress.ContainsKey(targetName))
        {
            _objectiveProgress[targetName] += amount;
            Debug.Log($"퀘스트 [{QuestData.questTitle}] 진행도 업데이트: {targetName} ({_objectiveProgress[targetName]}/{GetTargetAmount(targetName)})");
        }
    }

    private bool CheckCompletion()
    {
        return QuestData.objectives.All(obj => _objectiveProgress[obj.targetName] >= obj.amount);
    }

    private int GetTargetAmount(string targetName)
    {
        return QuestData.objectives.FirstOrDefault(obj => obj.targetName == targetName)?.amount ?? 0;
    }
}
