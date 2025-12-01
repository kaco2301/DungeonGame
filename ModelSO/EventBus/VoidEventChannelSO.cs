using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Void Event Channel")]
public class VoidEventChannelSO : EventChannelBaseSO
{
    public UnityAction Listeners;
    public void RaiseEvent() => Listeners?.Invoke();
    public void Register(UnityAction listener) => Listeners += listener;
    public void Unregister(UnityAction listener) => Listeners -= listener;
}
