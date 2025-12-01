using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class UI_Panel : UI_Base
{
    public Define.UILayer layer;
    protected CanvasGroup _canvasGroup;
    protected Animator _animator;

    private static readonly int ActiveHash = Animator.StringToHash("Active");

    public override void Init()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _animator = GetComponent<Animator>();
    }

    public virtual void Show()
    {
        if (_animator != null)
        {
            _animator.SetBool(ActiveHash, true);
        }

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    public virtual void Hide()
    {
        if (_animator != null)
        {
            _animator.SetBool(ActiveHash, false);
        }

        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
}


