using System;
/// <summary>
/// Interactor(로직)가 UI(UIManager)에게
/// 상태 변경을 알리기 위한 전역 이벤트
/// </summary>
public static class UIEvents
{
    public static event Action<InteractableData> OnInteractableFound;
    public static event Action OnInteractableLost;
    public static event Action OnInteractionEnded; 
    public static event Action<float> OnInteractionHoldProgress;
    public static event Action OnInteractionHoldCanceled;

    public static void ReportInteractionEnded()
    {
        OnInteractionEnded?.Invoke();
    }

    public static void ReportInteractableFound(InteractableData data)
    {
        OnInteractableFound?.Invoke(data);
    }

    public static void ReportInteractableLost()
    {
        OnInteractableLost?.Invoke();
    }

    public static void ReportInteractionHoldProgress(float amount) { OnInteractionHoldProgress?.Invoke(amount); }
    public static void ReportInteractionHoldCanceled() { OnInteractionHoldCanceled?.Invoke(); }
}
