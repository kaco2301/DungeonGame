using System.Collections.Generic;
using System.Linq;

public class PopupManager
{
    private readonly Stack<UI_Popup> _stack = new();
    private readonly int _order = 10;
    
    public UI_Popup Current => _stack.Count > 0 ? _stack.Peek() : null;
    public UI_Popup ShowPopup<TData>(UI_Popup popup, TData data) where TData : IPanelData
    {
        if (popup == null || popup.IsOpen) return null;

        if (popup is IDataPanel<TData> dataPanel)
            dataPanel.SetData(data);

        popup.Show();
        _stack.Push(popup);
        UpdatePopupOrder();

        return popup;
    }

    public UI_Popup ShowPopup(UI_Popup popup) => ShowPopup<IPanelData>(popup, default);

    public UI_Popup CloseCurrentPopup()
    {
        if (_stack.Count == 0) return null;

        UI_Popup close = _stack.Pop();
        UpdatePopupOrder();
        close.Hide();

        return close;
    }

    public void CloseAllPopup()
    {
        while (_stack.Count > 0)
        {
            _stack.Pop().Hide();
        }
    }

    private void UpdatePopupOrder()
    {
        int order = _order;

        foreach (var panel in _stack.Reverse())
        {
            panel.SetSortOrder(order++);
        }
    }
}
