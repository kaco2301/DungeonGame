using System;
using System.Collections.Generic;


public class ModalManager
{
    private readonly Stack<UI_Modal> _stack = new();

    public event Action<UI_Modal> OnModalOpened;
    public event Action<UI_Modal> OnModalClosed;

    //Stack 최상단 반환
    public UI_Modal Current => _stack.Count > 0 ? _stack.Peek() : null;

    public void ShowModal(UI_Modal modal)
    {
        if (modal == null) return;

        if (ReferenceEquals(Current, modal))
            return;

        if (_stack.Count > 0)
            _stack.Peek().OnSuspend();
        

        modal.Show();
        _stack.Push(modal);

        OnModalOpened?.Invoke(modal);
    }

    public void ShowModal<TData>(UI_Modal modal, TData data) where TData : IPanelData
    {
        if (modal == null) return;

        if (ReferenceEquals(Current, modal))
        {
            if (modal is IDataPanel<TData> dataPanel)
                dataPanel.SetData(data);


            return;
        }

        // 다른 모달을 띄우는 경우 (예: 일시정지 -> 설정창)
        if (_stack.Count > 0)
            _stack.Peek().OnSuspend();
        

        if (modal is IDataPanel<TData> dp)
            dp.SetData(data);
        

        modal.Show();
        _stack.Push(modal);

        OnModalOpened?.Invoke(modal);
    }


    public void CloseCurrentModalPanel()
    {
        if (_stack.Count == 0) return;

        UI_Modal panel = _stack.Pop();
        panel.Hide();

        OnModalClosed?.Invoke(panel);

        if (_stack.Count > 0)
        {
            _stack.Peek().OnResume();
        }
    }

    public void CloseAll()
    {
        while (_stack.Count > 0)
        {
            var panel = _stack.Pop();
            panel.Hide();
        }
    }


}
