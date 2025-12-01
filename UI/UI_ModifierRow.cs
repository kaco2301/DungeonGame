using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ModifierRow : UI_Base
{
    protected enum Images { Icon }
    protected enum Texts { ModifierText }

    private Image _icon => GetImage((int)Images.Icon);
    private TMP_Text _text => Get<TMP_Text>((int)Texts.ModifierText);

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
    }

    public void SetData(Sprite icon, string text)
    {
        if (_icon != null) _icon.sprite = icon;
        if (_text != null) _text.text = text;
        gameObject.SetActive(true); // 1. [신규] 오브젝트 활성화
    }

    public void ResetData()
    {
        if (_icon != null) _icon.sprite = null;
        if (_text != null) _text.text = "";
        gameObject.SetActive(false); // 2. [신규] 오브젝트 비활성화
    }
}
