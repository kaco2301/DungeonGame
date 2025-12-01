using System;
using System.Collections.Generic;
using UnityEngine;

public interface ITooltipUI
{
    Type HandledDataType { get; }

    void SetData(object data);
    void ShowTooltip(Vector2 position);
    void HideTooltip();
}

public class TooltipManager : MonoBehaviour
{
    [Header("Tooltip Prefabs")]
    [Tooltip("여기에 ItemTooltipUI, StatTooltipUI 등 모든 툴팁 Prefab을 등록하세요.")]
    [SerializeField] private List<GameObject> _tooltipPrefabs;

    [Header("Listening Channels")]
    [SerializeField] private TooltipEventChannelSO _showChannel;
    [SerializeField] private VoidEventChannelSO _hideChannel;

    // 데이터 타입(Key)과 생성된 툴팁 인스턴스(Value)를 매핑하는 딕셔너리
    private Dictionary<Type, ITooltipUI> _tooltipMap;

    // 현재 활성화된 툴팁
    private ITooltipUI _currentActiveTooltip = null;

    private void OnEnable()
    {
        if (_showChannel != null) _showChannel.Register(ShowTooltip);
        if (_hideChannel != null) _hideChannel.Register(HideTooltip);
    }

    private void OnDisable()
    {
        if (_showChannel != null) _showChannel.Unregister(ShowTooltip);
        if (_hideChannel != null) _hideChannel.Unregister(HideTooltip);
    }

    private void Awake()
    {
        _tooltipMap = new Dictionary<Type, ITooltipUI>();

        foreach (var prefab in _tooltipPrefabs)
        {
            if (prefab == null) continue;

            // 2. Prefab을 씬에 생성 (컨트롤러의 자식으로, 비활성화 상태로)
            GameObject instance = Instantiate(prefab, this.transform);
            ITooltipUI tooltip = instance.GetComponent<ITooltipUI>();

            if (tooltip != null)
            {
                // 3. 툴팁이 스스로 "처리할 수 있다"고 한 데이터 타입으로 딕셔너리에 등록
                Type dataType = tooltip.HandledDataType;

                if (dataType != null && !_tooltipMap.ContainsKey(dataType))
                {
                    _tooltipMap.Add(dataType, tooltip);
                    tooltip.HideTooltip(); // 확실하게 숨김
                }
            }
        }
    }

    /// <summary>
    /// [수정] 툴팁 표시 요청 처리 (타입 라우팅)
    /// </summary>
    private void ShowTooltip(TooltipEventData eventData)
    {
        if (eventData == null || eventData.Data == null)
        {
            HideTooltip();
            return;
        }

        // 5. 전달된 데이터의 Type을 가져옴 (예: InventoryItem 또는 StatTooltipData)
        Type dataType = eventData.Data.GetType();

        // 6. if/else if 분기문 대신, 딕셔너리에서 데이터 타입에 맞는 툴팁을 찾음
        if (_tooltipMap.TryGetValue(dataType, out ITooltipUI targetTooltip))
        {
            targetTooltip.SetData(eventData.Data);
            targetTooltip.ShowTooltip(eventData.ScreenPosition);
            _currentActiveTooltip = targetTooltip;
        }
        else
        {
            Debug.LogWarning($"[TooltipController] {dataType.Name} 타입을 처리할 툴팁이 등록되지 않았습니다."); 
            HideTooltip();
        }
    }

    /// <summary>
    /// [수정] 활성화된 툴팁 숨기기 (Destroy 대신 Hide)
    /// </summary>
    private void HideTooltip()
    {
        if (_currentActiveTooltip != null)
        {
            _currentActiveTooltip.HideTooltip();
            _currentActiveTooltip = null;
        }
    }
}
