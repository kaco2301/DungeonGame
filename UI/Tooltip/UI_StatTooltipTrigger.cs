using UnityEngine;
using UnityEngine.EventSystems;

public class UI_StatTooltipTrigger : UI_Base
{
    [SerializeField] private StatDataSO statData;

    [Header("Channels")]
    [SerializeField] private TooltipEventChannelSO _tooltipShowChannel;
    [SerializeField] private VoidEventChannelSO _tooltipHideChannel;

    private StatResult _currentResult;
    private CanvasGroup _rootPanelCanvasGroup;

    public override void Init()
    {
        _rootPanelCanvasGroup = GetComponentInParent<UI_Panel>()?.GetComponent<CanvasGroup>();
        BindEvent(gameObject, OnPointerEnter, Define.UIEvent.PointerEnter);
        BindEvent(gameObject, OnPointerExit, Define.UIEvent.PointerExit);
    }

    public void UpdateRuntimeStats(StatResult result)
    {
        _currentResult = result;
    }

    private void OnPointerEnter(PointerEventData eventData)
    {
        if (statData == null || _currentResult == null) return;

        StatBreakdown myBreakdown = null;
        if (_currentResult.Breakdown != null)
        {
            _currentResult.Breakdown.TryGetValue(statData.statType, out myBreakdown);
        }

        StatTooltipData dataPackage = new StatTooltipData(
            statData,
            _currentResult.FinalStat,
            myBreakdown
        );

        Vector2 tooltipPos = (Vector2)transform.position;
        _tooltipShowChannel?.RaiseEvent(new TooltipEventData(dataPackage, tooltipPos));

    }

    private void OnPointerExit(PointerEventData eventData)
    {
        _tooltipHideChannel.RaiseEvent();
    }

}
