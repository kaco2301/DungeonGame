using UnityEngine;
using UnityEngine.Events;

public abstract class EventChannelBaseSO : ScriptableObject { }

public abstract class EventChannelSO<T> : EventChannelBaseSO
{
    public UnityAction<T> Listeners;

    protected T _lastValue;
    protected bool _hasValue = false;

    public void RaiseEvent(T data)
    {
        _lastValue = data; // 값 저장 (기억)
        _hasValue = true;
        Listeners?.Invoke(data);
    }

    public void Register(UnityAction<T> listener)
    {
        Listeners += listener;

        if (_hasValue)
        {
            listener.Invoke(_lastValue);
        }
    }

    public void Unregister(UnityAction<T> listener)
    {
        Listeners -= listener;
    }
}


