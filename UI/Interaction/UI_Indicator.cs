using TMPro;
using UnityEngine.UI;

public class UI_Indicator : UI_Interaction
{
    protected enum Images
    {
        Icon,
    }

    protected enum Texts
    {
        ItemName,
        Action,
    }

    private Image Icon => GetImage((int)Images.Icon);
    private TMP_Text ItemName => Get<TMP_Text>((int)Texts.ItemName);
    private TMP_Text Action => Get<TMP_Text>((int)Texts.Action);

    public override void Init()
    {
        base.Init();
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
    }

    public override void SetData(InteractableData data)
    {
        Icon.sprite = data.Icon;
        ItemName.text = data.ItemName;
        Action.text = data.InteractionText;
    }

}
