using UnityEngine;

[RequireComponent (typeof(Canvas))]
public class UI_Scene : UI_Panel
{
    public bool IsOpen { get; private set; }

    public override void Init()
    {
        base.Init();
    }

    public override void Show()
    {
        base.Show();
        IsOpen = true;
    }

    private void OnEnable()
    {
        IsOpen = true;
    }

    private void OnDisable()
    {
        IsOpen = false;
    }

    public override void Hide()
    {
        base.Hide();
        IsOpen = false;
    }

    public virtual void Toggle()
    {
        if (IsOpen)
            Hide();
        else
            Show();
    }
}