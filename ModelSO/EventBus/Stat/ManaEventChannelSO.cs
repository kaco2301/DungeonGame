using UnityEngine;

[System.Serializable]
public struct ManaData
{
    public float Current;
    public float Max;

    public ManaData(float current, float max)
    {
        Current = current;
        Max = max;
    }
}

[CreateAssetMenu(menuName = "Events/Mana Event Channel")]
public class ManaEventChannelSO : EventChannelSO<ManaData>
{
}
