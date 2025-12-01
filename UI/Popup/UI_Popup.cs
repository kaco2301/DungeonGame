using UnityEngine;

[RequireComponent(typeof(Canvas))]
public abstract class UI_Popup : UI_Panel
{
    public bool IsOpen { get; private set; }
    public virtual bool IsInteractionPopup => false;
    private Canvas _canvas;

    public override void Init()
    {
        base.Init();
        _canvas = GetComponent<Canvas>();
    }

    public override void Show()
    {
        base.Show();
        IsOpen = true;
    }

    public override void Hide()
    {
        base.Hide();
        IsOpen = false;
    }

    public void SetSortOrder(int order)
    {
        if (_canvas != null)
        {
            _canvas.sortingOrder = order;
        }
    }
}
