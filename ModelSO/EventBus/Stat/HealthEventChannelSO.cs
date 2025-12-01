using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct HealthData
{
    public float Current;
    public float Max;

    public HealthData(float current, float max)
    {
        Current = current;
        Max = max;
    }

}
[CreateAssetMenu(menuName = "Events/Health Event Channel")]
public class HealthEventChannelSO : EventChannelSO<HealthData>
{
}
