using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Event Bus Container")]
public class EventBusSO : ScriptableObject
{
    [Tooltip("이 컨테이너에 포함될 채널들을 드래그하세요. 단, 동일한 타입의 채널이 중복되면 안 됩니다.")]
    [SerializeField] private List<EventChannelBaseSO> _eventChannels;

    private Dictionary<Type, EventChannelBaseSO> _lookup;

    private void OnEnable()
    {
        InitializeLookup();
    }

    // 런타임에 동적으로 초기화가 필요할 때를 대비해 public으로 분리
    public void InitializeLookup()
    {
        _lookup = new Dictionary<Type, EventChannelBaseSO>();

        if (_eventChannels == null) return;

        foreach (var channel in _eventChannels)
        {
            if (channel == null) continue;

            Type type = channel.GetType();

            if (_lookup.ContainsKey(type))
            {
                Debug.LogError($"[EventBusContainer] 중복된 타입 감지! {type.Name}이(가) 이미 존재합니다. " +
                               $"GetChannel<{type.Name}>() 호출 시 모호함이 발생하므로, 별도의 상속 클래스를 만드세요.");
                continue;
            }

            _lookup[type] = channel;
        }
    }

    public T GetChannel<T>() where T : EventChannelBaseSO
    {
        if (_lookup == null) InitializeLookup();

        if (_lookup.TryGetValue(typeof(T), out var result))
        {
            return result as T;
        }

        Debug.LogError($"[EventBusContainer] {typeof(T).Name} 타입의 채널을 찾을 수 없습니다! 리스트에 등록되었는지 확인하세요.");
        return null;
    }
}