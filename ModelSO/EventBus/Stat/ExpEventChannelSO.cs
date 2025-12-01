using UnityEngine;

[System.Serializable]
public struct ExpData
{
    public int CurrentExp;
    public int MaxExp; // 다음 레벨업 요구량

    public ExpData(int current, int max)
    {
        CurrentExp = current;
        MaxExp = max;
    }
}

[CreateAssetMenu(menuName = "Events/Exp Event Channel")]
public class ExpEventChannelSO : EventChannelSO<ExpData>
{
}
