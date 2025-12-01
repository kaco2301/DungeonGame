using UnityEngine;

public interface IModalState
{
    void OnSuspend(); // 새로운 모달이 위에 뜰 때
    void OnResume();  // 위 모달이 닫히고 다시 드러날 때
}

public abstract class UI_Modal : UI_Panel
{

    public virtual void OnSuspend()
    {
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    public virtual void OnResume()
    {
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }
}