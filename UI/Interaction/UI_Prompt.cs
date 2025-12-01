using TMPro;
using UnityEditor.UIElements;
using UnityEngine.UI;

public class UI_Prompt : UI_Interaction
{
    protected enum Texts
    {
        Action
    }

    protected enum Images
    {
        PressAndHold
    }

    private TMP_Text Action => Get<TMP_Text>((int)Texts.Action);
    private Image PressAndHold => Get<Image>((int)Images.PressAndHold);

    public override void Init()
    {
        base.Init();
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));

        if(PressAndHold != null)
        {
            if(PressAndHold.type != Image.Type.Filled)
            {
                return;
            }
            PressAndHold.fillAmount = 0;
        }
    }

    public override void SetData(InteractableData data)
    {
        if (Action != null) Action.text = data.InteractionText;
        if (PressAndHold != null)
        {
            PressAndHold.fillAmount = 0;
        }
    }

    public void UpdateFill(float amount)
    {
        if (PressAndHold == null) return;
        
        PressAndHold.fillAmount = amount;
    }

    public void CancelFill()
    {
        if (PressAndHold == null) return;
        PressAndHold.fillAmount = 0;
    }
}
